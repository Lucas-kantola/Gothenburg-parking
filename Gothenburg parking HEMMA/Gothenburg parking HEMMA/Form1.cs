using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace Gothenburg_parking_HEMMA
{

    public partial class Form1 : Form
    {


        /*
         * The datastructure that is stored in the array parkingData is 35 char long and does not contain ? characters. 
         * Where the first 10 char is the REG number followed by ":" and then type stored with 3 char: "MC?" or "CAR
         * after which there is a "@" to separate and then the Datetime.Now data with a serperator "&" at the end
         * XXXXXX:TYP@YEAR-Mo-Da ho:mi:se&
         *  REG   type       Datetime
         */


        string[] parkingData = new string[30];
        Regex extractPlate = new Regex(@"(?<=&|^).*?(?=:)");
        bool firstChangetbx = true;
        int prevPos;
        DoubleClickButton lastPressed;
        BindingList<string> platesList = new BindingList<string>();
        BindingList<string> MCs = new BindingList<string>();
        BindingList<string> CARs = new BindingList<string>();

        public Form1()
        {
            InitializeComponent();
            //InitTimer();
            comboboxMove.DataSource = platesList;
            comboBoxDepart.DataSource = platesList;
            lbCar.DataSource = CARs;
            lbMC.DataSource = MCs;
        }


        // --------------------------------------------------------------------------------
        // --------------------------------- Button setup ---------------------------------
        // --------------------------------------------------------------------------------
        List<DoubleClickButton> buttons;

        private void Form1_Load(object sender, EventArgs e)
        {
            buttons = new List<DoubleClickButton> {
            spot1, spot2, spot3, spot4, spot5,
            spot6, spot7, spot8, spot9, spot10,
            spot11, spot12, spot13, spot14, spot15,
            spot16, spot17, spot18, spot19, spot20,
            spot21, spot22, spot23, spot24, spot25,
            spot26, spot27, spot28, spot29, spot30,
            };

            foreach(DoubleClickButton item in buttons)
            {
                item.DoubleClick += Item_DoubleClick;
                item.ForeColor = Color.Black;
            }
        }

        private void Item_DoubleClick(object sender, EventArgs e)
        {
            displayContent();
        }

        // --------------------------------------------------------------------------------
        // --------------------------------- timer setup ----------------------------------
        // --------------------------------------------------------------------------------

/*        private Timer timer1 = new Timer();
        public void InitTimer()
        {
            timer1.Interval = 100;
            timer1.Tick += new EventHandler(Timer1_Tick);
            timer1.Start();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            UPDATE();
        }*/

        // --------------------------------------------------------------------------------
        // --------------------------------- remove text ----------------------------------
        // --------------------------------------------------------------------------------

        private void btnPressed(object sender, EventArgs e)
        {
            lastPressed = (DoubleClickButton)sender;
            lblSpot.Text = "Parking Spot Marked: " + lastPressed.Name;
        }

        private void tbxtextChanged(object sender, EventArgs e)
        {
            firstChangetbx = false;
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (textboxREG.Text == "ABC123" && firstChangetbx)
            {
                textboxREG.Text = "";
                textboxREG.ForeColor = Color.Black;
            }
        }

        private static void fillREG(ref string REG)
        {
            //fill the string to len 10 with replacement
            int underflow = 10 - REG.Length;
            while (underflow > 0)
            {
                REG += '?';
                underflow--;
            }
        }

        // --------------------------------------------------------------------------------
        // --------------------------------- UPDATES --------------------------------------
        // --------------------------------------------------------------------------------

        private void updateList()
        {
            platesList.Clear();
            MCs.Clear();
            CARs.Clear();
            for (int i = 0; i < parkingData.Length; i++)
            {
                if (parkingData[i] != null)
                {
                    MatchCollection collection = extractPlate.Matches(parkingData[i]);
                    for (int j = 0; j < collection.Count; j++)
                    {
                        string trimmed = Regex.Replace(collection[j].ToString(), @"\?", "");
                        if (!platesList.Contains(trimmed) && trimmed.Length > 0)
                        {
                            platesList.Add(trimmed);
                        }
                    }
                    //update vehicle lists
                    if (parkingData[i].Substring(11, 3) == "MC?")
                    {
                        MCs.Add(Regex.Replace(collection[0].ToString(), @"\?", ""));
                    }
                    else if (parkingData[i].Length > 35 && parkingData[i].Substring(46, 3) == "MC?"){
                        MCs.Add(Regex.Replace(collection[1].ToString(), @"\?", ""));
                    }
                    else if (parkingData[i].Substring(11, 3) == "CAR")
                    {
                        CARs.Add(Regex.Replace(collection[0].ToString(), @"\?", ""));
                    }
                }
            }
        }

        private void updateVisual()
        {
            for (int i = 0; i < parkingData.Length; i++)
            {
                if (parkingData[i] == null)
                {
                    buttons[i].BackColor = Color.Transparent;
                    buttons[i].Text = "";
                }

                if (parkingData[i] != null && parkingData[i].Substring(11, 3) == "CAR")
                {
                    buttons[i].BackColor = Color.Red;
                    buttons[i].Text = "1 CAR";
                }
                else if (parkingData[i] != null && parkingData[i].Length > 36)
                {
                    buttons[i].BackColor = Color.Red;
                    buttons[i].Text = "2 MC";
                }
                else if (parkingData[i] != null
                    && parkingData[i].Substring(11, 3) == "MC?"
                    && parkingData[i].Length < 36)
                {
                    buttons[i].BackColor = Color.Yellow;
                    buttons[i].Text = "1 MC";
                }
            }
        }


        //check caller for type of instruction and get the reg and position from the callers info
        private void updateInstructions(int? from, int? to, string REG)
        {
            string caller = new StackFrame(1, true).GetMethod().Name;
            to++;
            from++;

            if (REG.Contains('?'))
            {
                REG = REG.Replace("?", "");
            }

            if (caller == "moveVehicle")
            {
                tbxInstructions.Text += string.Format("Please move {0} from {1} to spot {2}\r\n", REG, from, to);
            }
            else if (caller == "Park")
            {
                tbxInstructions.Text += string.Format("Please park {0} at spot {1}\r\n", REG, to);
            }
            else if (caller == "depart")
            {
                tbxInstructions.Text += string.Format("Please remove {0} from spot {1}", REG, from);
            }
        }

        private void displayContent()
        {
            string data = parkingData[int.Parse(Regex.Replace(lastPressed.Name, @"spot", "")) - 1];

            if (lastPressed.Text != "" && data != null)
            {
                if (data.Length > 35)
                {
                    MatchCollection plate = extractPlate.Matches(data);
                    string message = string.Format(
                        "The space contains: \r\n" +
                        "     {0} \r\n" +
                        "     {1}", plate[0].ToString().Replace('?', ' '), plate[1]).Replace('?', ' ');
                    MessageBox.Show(message, "The spot contains:");
                }
                else
                {
                    string plate = extractPlate.Match(data).ToString().Replace('?', ' ');
                    string message = string.Format(
                        "The space contains: \r\n" +
                        "     {0}", plate);
                    MessageBox.Show(message, "The spot contains:");
                }

            }

        }

        private void UPDATE()
        {
            updateList();
            updateVisual();
        }

        // --------------------------------------------------------------------------------
        // --------------------------------- BASIC CHECKS ---------------------------------
        // --------------------------------------------------------------------------------

        private string getPlate(string data, bool first)
        {
            if (first)
            {
                return data.Substring(0, 10);
            }
            else if (!first)
            {
                return data.Substring(35, 10);
            }
            throw new Exception("getPlate ERROR");
        }

        private string getType(string data, bool first)
        {
            if(data != null)
            {
                if (first)
                {
                    return data.Substring(11, 3);
                }
                else if (data.Length > 36 && !first)
                {
                    return data.Substring(46, 3);
                }
            }
            throw new Exception("TYPE COULD NOT BE FOUND");
        }

        private bool isAlreadyParked(string REG)
        {
            foreach(string item in parkingData)
            {
                if(item != null)
                {
                    if (getPlate(item, true) == REG || (item.Length > 36 && getPlate(item, false) == REG))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private int spotTaken(int position, bool isCar)
        {

            if (isCar && parkingData[position] == null)
            {
                return 1;
            }
            else if ((
                !isCar && parkingData[position] == null) ||
                (parkingData[position].Substring(11, 3) != "CAR" && !isCar))
            {
                if (parkingData[position] == null || parkingData[position].Length < 36)
                {
                    return 2;
                }
            }

            return 0;
        }

        private string findIndexOfVehicle(string licens)
        {
            //returns:
            // index:1 == index är på första delen
            // index:2 == index är på andra delen
            if (isAlreadyParked(licens))
            {
                for (int i = 0; i < parkingData.Length; i++)
                {
                    if (parkingData[i] != null && parkingData[i].Substring(0, 10) == licens)
                    {
                        return string.Format("{0}:1", i);
                    }
                    else if (parkingData[i] != null && parkingData[i].Length > 35 && parkingData[i].Substring(35, 10) == licens)
                    {
                        return string.Format("{0}:2", i);
                    }
                }
                throw new Exception("VEHICLE HAS NO INDEX");
            }
            throw new Exception("VEHICLE IS NOT IN ARRAY");
        }

        private void placeVehicle(int position, string licens, bool isCar)
        {
            string type = isCar ? "CAR" : "MC?";

            parkingData[position] += string.Format("{0}:{1}@{2}&", licens, type, DateTime.Now);
        }

        private int Park(string licens, bool isCar)
        {
            //returns:
            //  0: failed to park
            //  1: parkerd
            //  2: already parked

            if (isAlreadyParked(licens))
            {
                return 2;
            }
            else
            {
                for (int i = 0; i < parkingData.Count(); i++)
                {
                    int spotStorage = spotTaken(i, isCar);
                    if (spotStorage == 1)
                    {
                        placeVehicle(i, licens, isCar);
                        prevPos = i + 1;
                        updateInstructions(null , i + 1, licens);
                        return 1;
                    }
                    else if (spotStorage == 2)
                    {
                        placeVehicle(i, licens, isCar);
                        prevPos = i + 1;
                        updateInstructions(null, i + 1, licens);
                        return 1;
                    }
                }
            }
            return 0;
        }

        private void moveVehicle(string REG, int moveTo)
        {
            if (REG != "")
            {
                //fill the reg to 10 char with "?" as replacements
                fillREG(ref REG);

                //safty check
                if(isAlreadyParked(REG) == false)
                {
                    lblMove.Text = "Vehicle not in parkinglot";
                }

                //find position of REG 
                string index = findIndexOfVehicle(REG);
                string[] indexParts = index.Split(':');
                int position = int.Parse(indexParts[0]);
                int partIndex = int.Parse(indexParts[1]);

                //get type of REG that is getting parked
                bool isCar = getType(parkingData[position], true) == "CAR";

                //get the plate of the REG
                MatchCollection plates = extractPlate.Matches(parkingData[position]);

                if (parkingData[moveTo] == null)
                {
                    if (!isCar)
                    {
                        if (partIndex == 1)
                        {
                            if (parkingData[position].Length > 35)
                            {
                                string temp = parkingData[position].Substring(35, 35);
                                placeVehicle(moveTo, plates[partIndex - 1].ToString(), isCar);
                                Array.Clear(parkingData, position, 1);
                                parkingData[position] = temp;
                                UPDATE();
                                lblMove.Text = "Vehicle moved succesfully";
                                updateInstructions(position, moveTo, plates[partIndex - 1].ToString());
                                return;
                            }
                            else
                            {
                                placeVehicle(moveTo, plates[partIndex - 1].ToString(), isCar);
                                Array.Clear(parkingData, position, 1);
                                UPDATE();
                                lblMove.Text = "Vehicle moved succesfully";
                                updateInstructions(position , moveTo, plates[partIndex - 1].ToString());
                                return;
                            }

                        }
                        else if (partIndex == 2)
                        {
                            string temp = parkingData[position].Substring(0,35);
                            placeVehicle(moveTo, plates[1].ToString(), isCar);
                            Array.Clear(parkingData, position, 1);
                            parkingData[position] = temp;
                            UPDATE();
                            lblMove.Text = "Vehicle moved succesfully";
                            updateInstructions(position, moveTo, plates[partIndex - 1].ToString());
                            return;
                        }
                    }

                    else if (isCar)
                    {
                        placeVehicle(moveTo, plates[partIndex - 1].ToString(), isCar);
                        Array.Clear(parkingData, position, 1);
                        UPDATE();
                        lblMove.Text = "Vehicle moved succesfully";
                        updateInstructions(position, moveTo, plates[partIndex - 1].ToString());
                        return;
                    }
                }

                else if (!isCar && getType(parkingData[moveTo], true) == "MC?" && parkingData[moveTo].Length <= 35 && parkingData[position] != parkingData[moveTo])
                {
                    if (partIndex == 1)
                    {
                        if (parkingData[position].Length > 35)
                        {
                            string temp = parkingData[position].Substring(35, 35);
                            placeVehicle(moveTo, plates[partIndex - 1].ToString(), isCar);
                            Array.Clear(parkingData, position, 1);
                            parkingData[position] = temp;
                            UPDATE();
                            lblMove.Text = "Vehicle moved succesfully";
                            updateInstructions(position, moveTo, plates[partIndex - 1].ToString());
                            return;
                        }
                        else
                        {
                            placeVehicle(moveTo, plates[partIndex - 1].ToString(), isCar);
                            Array.Clear(parkingData, position, 1);
                            UPDATE();
                            lblMove.Text = "Vehicle moved succesfully";
                            updateInstructions(position, moveTo, plates[partIndex - 1].ToString());
                            return;
                        }

                    }
                    else if (partIndex == 2)
                    {
                        string temp = parkingData[position].Substring(0, 35);
                        placeVehicle(moveTo, plates[1].ToString(), isCar);
                        Array.Clear(parkingData, position, 1);
                        parkingData[position] = temp;
                        UPDATE();
                        lblMove.Text = "Vehicle moved succesfully";
                        updateInstructions(position, moveTo, plates[partIndex - 1].ToString());
                        return;
                    }
                }


                else
                {
                    lblMove.Text = "Vehicle could not be moved: Space occupied";
                    return;
                }
            }
            else
            {
                throw new Exception("'VEHICLE' IS EMPTY");
            }
        }

        private void depart(string REG)
        {

            if(isAlreadyParked(REG) == false)
            {
                throw new Exception("Vehicle is not parked and cannot be checked out");
            }

            //find position of REG 
            string index = findIndexOfVehicle(REG);
            string[] indexParts = index.Split(':');
            int position = int.Parse(indexParts[0]);
            int partIndex = int.Parse(indexParts[1]);

            if (parkingData[position].Length <= 35)
            {
                Array.Clear(parkingData, position, 1);
                lblDepartRes.Text = "Vehicle has been depared";
                Update();
                updateInstructions(position, null, REG);
            }
            else if (parkingData[position].Length > 35 && partIndex == 1)
            {
                string temp = parkingData[position].Substring(35, 35);
                Array.Clear(parkingData, position, 1);
                parkingData[position] = temp;
                Update();
                updateInstructions(position, null, REG);
            }
            else if (parkingData[position].Length > 35 && partIndex == 2)
            {
                string temp = parkingData[position].Substring(0, 35);
                Array.Clear(parkingData, position, 1);
                parkingData[position] = temp;
                Update();
                updateInstructions(position, null, REG);
            }
            else
            {
                throw new Exception("Vehicle could not be depared");
            }

        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            if (lastPressed != null && comboboxMove.Text != "")
            {
                string REG = comboboxMove.Text;
                int moveTo = int.Parse(Regex.Replace(lastPressed.Name, @"spot", "")) - 1;
                moveVehicle(REG, moveTo);
            }
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string regnr = textboxREG.Text;
            bool isCar = radioCAR.Checked;

            if (regnr.Contains("?"))
            {
                labelVehicleParked.Text = "Please insert valid reg";
                return;
            }

            if (regnr.Length < 10 && regnr.Length > 0)
            {
                fillREG(ref regnr);
            }
            if (regnr.Length > 0)
            {
                switch (Park(regnr, isCar))
                {
                    case 0:
                        labelVehicleParked.Text = Regex.Replace(regnr, @"\?", "") + " Has failed to park";
                        break;

                    case 1:
                        labelVehicleParked.Text = Regex.Replace(regnr, @"\?", "") + " Has been parked at " + prevPos;
                        break;

                    case 2:
                        labelVehicleParked.Text = Regex.Replace(regnr, @"\?", "") + " Is already parked";
                        break;
                }
            }
            UPDATE();
        }

        private void btnDepart_Click(object sender, EventArgs e)
        {
            if (comboBoxDepart.Text != "")
            {
                string REG = comboBoxDepart.Text;
                fillREG(ref REG);
                depart(REG);
            }
        }
    }
}
