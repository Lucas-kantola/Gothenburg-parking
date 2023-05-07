using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Schema;

namespace Gothenburg_parking_HEMMA
{
    public partial class Form1 : Form
    {

        string[] parkingdata = new string[30];
        Regex exctractPlate = new Regex(@"(^|(?<=\&))[A-Za-z\d]{0,10}\?{0,10}");
        //Regex exctractRight = new Regex(@"(?=\&)[A-Za-z\d]{0,10}\?{0,10}");
        //Regex exctractLeft = new Regex(@"^[A-Za-z\d]{0,10}\?{0,10}");
        bool firstChangetbx = true;
        int prevPos;
        Button lastPressed;
        BindingList<string> platesList = new BindingList<string>();


        public Form1()
        {
            InitializeComponent();
        }

        List<Button> buttons;

        private void Form1_Load(object sender, EventArgs e)
        {
            buttons = new List<Button> {
            spot1, spot2, spot3, spot4, spot5,
            spot6, spot7, spot8, spot9, spot10,
            spot11, spot12, spot13, spot14, spot15,
            spot16, spot17, spot18, spot19, spot20,
            spot21, spot22, spot23, spot24, spot25,
            spot26, spot27, spot28, spot29, spot30,
            };
        }

        private void btnPressed(object sender, EventArgs e)
        {
            lastPressed = (Button)sender;
        }

        private void tbxtextChanged(object sender, EventArgs e)
        {
            firstChangetbx = false;
        }

        private void updateList()
        {
            for (int i = 0; i < parkingdata.Length; i++)
            {
                if(parkingdata[i] != null)
                {
                    MatchCollection collection = exctractPlate.Matches(parkingdata[i]);
                    for (int j = 0; j < collection.Count; j++)
                    {
                        string trimmed = Regex.Replace(collection[j].ToString(), @"\?", "");
                        if (!platesList.Contains(trimmed) && trimmed.Length > 0)
                        {
                            platesList.Add(trimmed);
                        }

                    }
                }
            }
        }

        private void updateVisual()
        {
            for (int i = 0; i < parkingdata.Length; i++)
            {
                if(parkingdata[i] == null)
                {
                    buttons[i].BackColor = Color.Transparent;
                }

                if (parkingdata[i] != null && parkingdata[i].Substring(11, 3) == "CAR")
                {
                    buttons[i].BackColor = Color.Red;
                    buttons[i].Text = "1 CAR";
                }
                else if (parkingdata[i] != null && parkingdata[i].Length > 35)
                {
                    buttons[i].BackColor = Color.Red;
                    buttons[i].Text = "2 MC";
                }
                else if (parkingdata[i] != null
                    && parkingdata[i].Substring(11, 2) == "MC"
                    && parkingdata[i].Length < 35)
                {
                    buttons[i].BackColor = Color.Yellow;
                    buttons[i].Text = "1 MC";
                }
            }
        }

        private void UPDATE()
        {
            comboboxMove.DataSource = platesList;
            updateList();
            updateVisual();
        }

        private void textBox1_MouseClick(object sender, MouseEventArgs e)
        {
            if (textboxREG.Text == "ABC123" && firstChangetbx)
            {
                textboxREG.Text = "";
                textboxREG.ForeColor = Color.Black;
            }
        }

        private bool isAlreadyParked(string REG)
        {
            foreach (string item in parkingdata)
            {
                if (item != null && (item.Substring(0,10) == REG || (item.Length > 35 && item.Substring(34,10) == REG)))
                {
                    return true;
                }
            }
            return false;
        }

        private int spotTaken(int position, bool isCar)
        {

            if (isCar && parkingdata[position] == null)
            {
                return 1;
            }
            else if ((
                !isCar && parkingdata[position] == null) ||
                (parkingdata[position].Substring(11, 3) != "CAR" && !isCar))
            {
                if (parkingdata[position] == null || parkingdata[position].Length < 35)
                {
                    return 2;
                }
            }
           
            return 0;

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
                for (int i = 0; i < parkingdata.Count(); i++)
                {
                    int spotStorage = spotTaken(i, isCar);
                    if(spotStorage == 1)
                    {
                        parkingdata[i] = string.Format("{0}:{1}@{2}", licens, "CAR", DateTime.Now);
                        prevPos = i + 1;
                        return 1;
                    }
                    else if (spotStorage == 2)
                    {
                        parkingdata[i] += string.Format("{0}:{1}@{2}&", licens, "MC", DateTime.Now);
                        prevPos = i + 1;
                        return 1;
                    }
                }
            }
            return 0;
        }

        private void moveVehicle(string vehicle, int moveTo)
        {
            if(vehicle != "")
            {
                int underflow = 10 - vehicle.Length;
                while (underflow > 0)
                {
                    vehicle += '?';
                    underflow--;
                }

                Console.WriteLine(vehicle + "1");

                for(int i = 0; i < parkingdata.Length; i++)
                {
                    if (parkingdata[i] != null)
                    {
                        Console.WriteLine(Regex.Replace(parkingdata[i].Substring(0, 10), @"\?", "") + "54");
                    }

                    if (parkingdata[i] != null && (parkingdata[i].Substring(0, 10) == vehicle
                        || (parkingdata[i].Length > 35 && exctractPlate.Replace(parkingdata[i].Substring(34, 10), "") == vehicle)))
                    {
                        Console.WriteLine(exctractPlate.Matches(parkingdata[i])[0].ToString() + "2");
                        if (parkingdata[i].Length > 35)
                        {
                            parkingdata[moveTo - 1] = parkingdata[i];
                            parkingdata[i] = null;
                            UPDATE();
                        }
                        else if (exctractPlate.Matches(parkingdata[i])[0].ToString() == vehicle)
                        {
                            Console.WriteLine(parkingdata[i].Substring(0, 34));
                            parkingdata[moveTo] = parkingdata[i].Substring(0, 34);
                            Console.WriteLine("added and removed @ parkingdata[i][0]");
                            platesList[platesList.IndexOf(parkingdata[i].Substring(0,10))] = null;
                            parkingdata[i] = null;
                            UPDATE();
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("hej");
            }
        }

        private void btnMove_Click(object sender, EventArgs e)
        {
            if(lastPressed != null)
            {
                string vehicle = comboboxMove.Text;
                int moveTo = int.Parse(Regex.Replace(lastPressed.Name, @"spot", ""));
                moveVehicle(vehicle, moveTo);
            }
        }

        private void displayContent()
        {
            groupBox3.Text = parkingdata[int.Parse(Regex.Replace(lastPressed.Name, @"spot", ""))];
        }

        private void btnRegister_Click(object sender, EventArgs e)
        {
            string regnr = textboxREG.Text;
            bool isCar = radioCAR.Checked;
            if (regnr.Length < 10 && regnr.Length > 0)
            {
                int underflow = 10 - regnr.Length;
                while (underflow > 0)
                {
                    regnr += '?';
                    underflow--;
                }
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


            //updates
            UPDATE();

            //bugtesting
            foreach (string item in parkingdata)
            {
                Console.WriteLine(item);
            }
            foreach (string item in platesList)
            {
                Console.WriteLine(item);
            }
            displayContent();
        }
    }
}
