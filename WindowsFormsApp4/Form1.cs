﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Threading;



namespace WindowsFormsApp4
{
    public partial class Form1 : Form
    {
        //class Form1 inherits from Form
        //Form1 is responsible of the game screen
        
        private TcpClient client; //The client defined in the main program
        private NetworkStream stream; //The stream defined in the main program
        private Label[] boardLabels; //Array of the labels that make up the game board
        private string mode;

        public Form1(TcpClient client, NetworkStream stream, string mode = "")
        {
            //The contractor of the game screen
            //arg: client, stream

            InitializeComponent();

            Label[] boardLabels = { this.hole0,this.hole1,this.hole2,this.hole3,
                this.hole4,this.hole5,this.hole6,this.hole7,this.hole8,this.hole9,
                this.hole10,this.hole11,this.hole12,this.hole13};

            this.boardLabels = boardLabels;
            this.client = client;
            this.stream = stream;
            this.mode = mode;
        }

        private string receiveInfo()
        {
            //Performs back-up communication and verifies the correctness of the message
            //ret = responseData

            //Receives message length (set to five chars)
            Byte[] data = new Byte[5]; //the response ASCII representation
            Int32 bytes = stream.Read(data, 0, data.Length);
            string responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

            //A loop that is performed until the length of the message is received in full
            while (responseData.Length<5)
            {
                data = new Byte[5-responseData.Length]; 
                bytes = stream.Read(data, 0, data.Length);
                responseData =responseData+
                    System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            }

            Console.WriteLine(responseData);

            //Receives the message
            data = new Byte[int.Parse(responseData)];
            bytes = stream.Read(data, 0, data.Length);
            responseData = System.Text.Encoding.ASCII.GetString(data, 0, bytes);

            //A loop that is performed until the message is received in full
            while (responseData.Length<data.Length)
            {
                data = new Byte[data.Length - responseData.Length];
                bytes = stream.Read(data, 0, data.Length);
                responseData = responseData +
                    System.Text.Encoding.ASCII.GetString(data, 0, bytes);
            }

            return responseData;
        }

        private void Game()
        {
            //Performs backend communication to update the board

            //Receives game ID
            string responseData = receiveInfo();
            responseData = receiveInfo();

            //Counts the number of games played- relevant to competition mode
            int countGame=0;

            //A loop that performs six games in a competition.
            while ((mode == "competition" && countGame < 6) || (mode != "competition" && countGame < 1)) 
            {
                labelTitle.Text = "Game Started";
                labelTitle.Update();

                //A loop that lasts until the end of the game and updates the board
                while (responseData != "LOSS" && responseData != "WIN")
                {
                    Console.WriteLine(responseData);
                    int index = responseData.IndexOf('[');
                    responseData = responseData.Remove(index, 1);
                    index = responseData.IndexOf(']');
                    responseData = responseData.Remove(index, 1);

                    responseData = responseData.Replace(",", "");
                    Console.WriteLine(responseData);

                    string[] update = responseData.Split(' ');

                    string[] updateBoardLabels = new string[14];

                    for (int i = 0; i < updateBoardLabels.Length; i++)
                    {
                        updateBoardLabels[i] = update[i] + "";
                    }

                    UpdateBoard(updateBoardLabels, update[update.Length - 1]);

                    responseData = receiveInfo();
                }

                //If the user wins changes the screen title to "YOU WON!", else "YOU LOST:("
                if (responseData == "WIN")
                    labelTitle.Text = "YOU WON!";
                else
                    labelTitle.Text = "YOU LOST :(";
            }

            //Returns all holes to their original color
            for (int i = 0; i < boardLabels.Length; i++)
            {
                this.boardLabels[i].BackColor = Color.Tan;
                this.boardLabels[i].Update();

            }

            countGame++;
        }

        private void UpdateBoard(string [] updateArr,string isMyTurn, int choice=0)
        {
            //Updating the board
            //arg: updateArr, isMyTurn, choice
            //updateArr contains the new values of the holes in the board
            //isMyTurn true if it's the user's turn, else false
            //choice stores the hole number from which the turn was made

            //Marks who is playing
            if (isMyTurn=="True")
            {
                this.checkBoxMe.Checked = true;
                this.checkBoxOpponent.Checked = false;
            }
            else
            {
                this.checkBoxMe.Checked = false;
                this.checkBoxOpponent.Checked = true;
            }

            //Returns all holes to their original color
            for (int i = 0; i < boardLabels.Length; i++)
            {
                this.boardLabels[i].BackColor = Color.Tan;
                this.boardLabels[i].Update();

            }

            
            for (int i = 0; i < boardLabels.Length; i++)
            {
                //Changes the color of holes which have changed
                if (boardLabels[i].Text!=updateArr[i])
                {
                    this.boardLabels[i].BackColor = Color.Tomato;
                    this.boardLabels[i].Update();
                }

                this.boardLabels[i].Text = updateArr[i];
                this.boardLabels[i].Update();
            }
            //Thread.Sleep(300); //optional
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            //Closes the entire program by clicking on the X
            //arg: e

            this.stream.Close();
            this.client.Close();
            Application.Exit();
        }

        private void returnToJoinstartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Returns to the previous screen by clicking on this option in the menu
            //arg: sender, e

            Form3 f = new Form3(client,stream);
            f.Location = this.Location;
            f.Show();
            this.Hide();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Closes the entire program by clicking on this option in the menu
            //arg: sender, e

            this.stream.Close();
            this.client.Close();
            Application.Exit();
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //Requests a new game and updates the screen accordingly.
            //arg: sender, e

            //Returns the board to its original position
            for (int i=0;i<this.boardLabels.Length;i++)
            {
                if (i == 0 || i == 7)
                    this.boardLabels[i].Text = "0";
                else this.boardLabels[i].Text = "4";
                boardLabels[i].Update();
            }

            this.checkBoxMe.Checked = false;
            this.checkBoxOpponent.Checked = false;

            Byte[] data = System.Text.Encoding.ASCII.GetBytes("restart");
            stream.Write(data, 0, data.Length);

            Game();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //starts to update the board when "ready?" button is pressed
            //arg: sender, e

            Game();
        }
    }
}
