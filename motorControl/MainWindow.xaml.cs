using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO.Ports;
using System.Diagnostics;

namespace motorControl {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        bool isConnected = false;
        String[] ports;
        SerialPort port;

        double currentWavelength;

        int currentIndex;

        int gearRatio = 90;

        int stepsPerCircle = 4000;

        string serialString;



        public MainWindow() {
            InitializeComponent();

            currentWavelength = Properties.Settings.Default.waveLength;

            currentIndex = Properties.Settings.Default.Index;


            getAvailableComPorts();

            foreach (string port in ports) {
                SerialList.Items.Add(port);

                if (ports[0] != null) {
                    SerialList.SelectedItem = ports[0];
                }
            }
        }

        void getAvailableComPorts() {
            ports = SerialPort.GetPortNames();
        }



        private void getWavelengthButton_Click(object sender, RoutedEventArgs e) {
            WavelengthTextbox.Text = currentWavelength.ToString("F3");
        }


        double getAngle(double pWavelength) {

            return Math.Asin(pWavelength / 1250);



        }

        double getWavelenth(double angle) {


            return Math.Sin(angle) * 1250;

        }

        private void startCaliberate_Click(object sender, RoutedEventArgs e) {

            double temp = 0;

            if (caliTextbox.Text != "") {

                try {
                    temp = Convert.ToDouble(caliTextbox.Text);

                } catch {

                    MessageBox.Show("Please enter a valid wavelength!");
                    return;

                }

                if (temp > 1050 || temp < 650) {

                    MessageBox.Show("Please enter a valid wavelength!");
                    return;
                }



                newWavelength(temp);
                WavelengthTextbox.Text = currentWavelength.ToString("F3");
                indexTextbox.Text = currentIndex.ToString();

            } else {

                MessageBox.Show("Please enter a valid wavelength!");
            }
        }

        private void getIndexButton_Click(object sender, RoutedEventArgs e) {


            indexTextbox.Text = currentIndex.ToString();

        }


        private void newWavelength(double pwavelength) {

            currentWavelength = pwavelength;

            currentIndex = getIndex(currentWavelength);

            Properties.Settings.Default.Index = currentIndex;
            Properties.Settings.Default.waveLength = currentWavelength;
            Properties.Settings.Default.Save();

        }

        private void newIndex(int pindex) {


            currentIndex = pindex;
            currentWavelength = getwavelength(currentIndex);

            Properties.Settings.Default.Index = currentIndex;
            Properties.Settings.Default.waveLength = currentWavelength;
            Properties.Settings.Default.Save();

        }

        private int getIndex(double pwavelength) {

            double myAngle = getAngle(pwavelength) - getAngle(650);


            return Convert.ToInt32((myAngle / 2 / Math.PI) * gearRatio * stepsPerCircle);

        }

        private double getwavelength(int pindex) {

            double angle = (Convert.ToDouble( pindex )/ gearRatio / stepsPerCircle) * 2 * Math.PI;


            return getWavelenth(angle+ getAngle(650)) ;


        }

        private void Button_Click(object sender, RoutedEventArgs e) {

            double temp = 0;

            if (goTextbox.Text != "") {

                try {
                    temp = Convert.ToDouble(goTextbox.Text);

                } catch {

                    MessageBox.Show("Please enter a valid wavelength!");
                    return;

                }

                if (temp > 1050 || temp < 650) {

                    MessageBox.Show("Please enter a valid wavelength!");
                    return;
                }

                int moveSteps = getIndex(temp) - currentIndex;

                sendMove(moveSteps);



                //newWavelength(temp);

            } else {

                MessageBox.Show("Please enter a valid wavelength!");
            }


        }

        private void sendMove(int steps) {

            if(port == null || !port.IsOpen) {

                MessageBox.Show("please connect");
                return;
            }
            

            string temp;
            if (steps > 0) {
                //"f|stpes;"

                temp = "f|" + steps.ToString() + ";";
               

            } else {
                //"b|-steps;"

                temp = "b|" + (0-steps).ToString() + ";";
            }

            Debug.WriteLine(temp);

            try {

                port.Write(temp);

            } catch {


                MessageBox.Show("Send command Error.");
            }
            newIndex(currentIndex + steps);
            WavelengthTextbox.Text = currentWavelength.ToString("F3");
            indexTextbox.Text = currentIndex.ToString();

        }

        private void fowardButton_Click(object sender, RoutedEventArgs e) {

            int temp;

            if (stepsTextbox.Text != "") {

                try {
                    temp = Convert.ToInt32(stepsTextbox.Text);

                } catch {

                    MessageBox.Show("Please enter a valid step number!");
                    return;

                }

                if (temp < 0) {
                    MessageBox.Show("Please enter a positive step number!");
                    return;

                }
                if (temp + currentIndex > getIndex(1050)) {
                    MessageBox.Show("out of range");
                    return;

                }
                sendMove(temp);


            } else {

                MessageBox.Show("Please enter a valid step number!");
                return;
            }
        }

        private void backButton_Click(object sender, RoutedEventArgs e) {

            int temp;

            if (stepsTextbox.Text != "") {

                try {
                    temp = Convert.ToInt32(stepsTextbox.Text);

                } catch {

                    MessageBox.Show("Please enter a valid step number!");
                    return;

                }

                if (temp < 0) {
                    MessageBox.Show("Please enter a positive step number!");
                    return;

                }
                if (currentIndex - temp < getIndex(650)) {
                    MessageBox.Show("out of range");
                    return;

                }
                sendMove(0 - temp);


            } else {

                MessageBox.Show("Please enter a valid step number!");
                return;
            }

        }

        private void refreshButton_Click(object sender, RoutedEventArgs e) {

            getAvailableComPorts();

            SerialList.Items.Clear();

            foreach (string port in ports) {
                SerialList.Items.Add(port);

                if (ports[0] != null) {
                    SerialList.SelectedItem = ports[0];
                }
            }

        }

        private void connectButton_Click(object sender, RoutedEventArgs e) {

            if (isConnected) {

                disconnect();

                connectButton.Content = "Connect";
                isConnected = false;


            } else {
                bool flag = connect();
                if (flag) {

                    connectButton.Content = "Disconnect";

                    isConnected = true;



                } else {

                    MessageBox.Show("Error cannot connect");
                    isConnected = false;


                }

            }

        }

        void disconnect() {
            try {

                port.Close();
                port.Dispose();


            } catch (Exception ee) {
                MessageBox.Show(ee.ToString());
            }
        }


        bool connect() {
            if (SerialList.SelectedItem == null) {

                return false;
            }
            string selectedPort = SerialList.SelectedItem.ToString();
            port = new SerialPort(selectedPort, 9600);

            try {
                port.Open();

                port.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(Recieve);
            } catch {

                return false;
            }

            return port.IsOpen;
        }


        private void Recieve(object sender, System.IO.Ports.SerialDataReceivedEventArgs e) {
            serialString += port.ReadExisting();

            while (serialString.Contains(';')) {

                string temp = serialString.Substring(0, serialString.IndexOf(';'));

                if(temp == "moving") {
                    this.Dispatcher.Invoke(() => {

                        statusE.Fill = Brushes.Green;

                    });

                   
                }else if(temp == "finish"){
                    this.Dispatcher.Invoke(() => {


                        statusE.Fill = Brushes.Red;

                    });


                }

                serialString = serialString.Substring(serialString.IndexOf(';') +1 );


            }


        }

    }
}
