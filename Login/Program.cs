﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using ConsoleGameLib;


using static System.Threading.Thread;
using System.Diagnostics;
using ConsoleGameLib.PhysicsTypes;


namespace Login
{
    class Program
    {
        static List<String> GetFiles(string path)
        {
            List<String> directories = new List<String>();

            directories.AddRange(Directory.GetFiles(path));

            foreach (string newPath in Directory.GetDirectories(path))
            {
                FileIOPermission perms = new FileIOPermission(FileIOPermissionAccess.Read, newPath);
                if (perms.IsUnrestricted())
                {
                    directories.AddRange(GetFiles(newPath));
                }
            }

            return directories;
        }

        static Random rand = new Random();
        static void Main(string[] args)
        {
            bool edit = false;
            int editID = 0;
            string alphabet = "abcdefghijklmnopqrstuvwxyz";
            Console.BufferHeight = 120;

            string currentPath = @"C:\Windows\System32";

            //Random rand = new Random();

            Dictionary<string, Dictionary<string, bool>> POSQs = new Dictionary<string, Dictionary<string, bool>>() {
                { "What part of speech is 'never' in the following sentence? I've never been late to school!", new Dictionary<string, bool> { { "Preposition", false }, { "Adverb", true }, { "Noun", false }, { "Verb", false } } },
                { "What part of speech is 'running' in the following sentence? Running is my favorite thing to do.", new Dictionary<string, bool> { { "Interjection", false }, { "Adjective", false }, { "Noun", true }, { "Verb", false } } },
                { "What part of speech is 'everyone' in the following sentence? Everyone likes Eddie's new elephant.", new Dictionary<string, bool> { { "Pronoun", true }, { "Noun", false }, { "Verb", false }, { "Interjection", false } } },
                { "What part of speech is 'therefore' in the following sentence? The criminal jumped the fence, therefore the owner made it taller.", new Dictionary<string, bool> { { "Adverb", false }, { "Conjunction", true }, { "Onomatopoeia", false }, { "Interjection", false } } }
            };
            Dictionary<string, Dictionary<string, bool>> AMQs = new Dictionary<string, Dictionary<string, bool>>() {
                { "What is 9 * 7?", new Dictionary<string, bool>() { { "81", false }, { "63", true }, { "16", false }, { "83",false } } },
                { "What is 3^3?", new Dictionary<string, bool>() { { "3", false }, { "9", false }, { "27", true }, { "1", false } } },
                { "What is 32/8?", new Dictionary<string, bool>() { { "4", true }, { "2", false }, { "6", false }, { "7", false } } },
                { "(8/9)^2 = ?", new Dictionary<string, bool>() { { "64/9", false }, { "8/(9^2)", false }, { "8/81", false }, { "64/81", true } } },
                { "4^(3/2) = ?", new Dictionary<string, bool>() { { "6", false }, { "~2.67", false }, { "~2.52", false }, { "8", true } } },
                { "1/4 * (4 - 1/2) = ?", new Dictionary<string, bool>() { { "1.125", false }, { "1/2", false }, { "3.5", false }, { "7/8", true } } }
            };
            Dictionary<string, Dictionary<string, bool>> GQs = new Dictionary<string, Dictionary<string, bool>>() {
                { "The phrase delimited by commas in the following sentence is called a/an ____________. Johny, an average American, enjoys walking his dog.", new Dictionary<string, bool>() { { "description", false }, { "antecendent", false }, { "appositive", true }, { "aqueduct", false } } },
                { "The noun a pronoun replaces is that pronoun's ___________.", new Dictionary<string, bool>() { { "noun", false }, { "antecendent", true }, { "appositive", false }, { "aqueduct", false } } }
            };
            Dictionary<string, Dictionary<string, bool>> ALGMQs = new Dictionary<string, Dictionary<string, bool>>() {
                { "Find x when (x + x^0.5)/7 = x^0.5 and x is not 0.", new Dictionary<string, bool>() { { "0", false }, { "6", false }, { "36", true }, { "7", false } } },
                { "Which equation describes a line perpendicular to y = 7x - 3 that passes through (8,4)?", new Dictionary<string, bool>() { { "y = (-1/7)x + 4/7", false }, { "y = (-1/7)x + 36/7", true }, { "y = (-1/7)x", false }, { "y = 7x + 4", false } }},
                { "x^((3x)^0.5) = 27", new Dictionary<string, bool>() { { "x = 3", true }, { "x = 9", false }, { "x = 27", false }, { "x = 36", false } } },
                { "xyz = 1/2(x/3)", new Dictionary<string, bool>() { { "z = 1/2 and y = 1/3", true }, { "x = -2 and y = 7", false }, { "z = 2 and y = 3", false }, { "y = 1/3x and z = 1/2x", false } } },
                { "Which line is parallel to y - 3 = 2(x - 5)?", new Dictionary<string, bool>() { { "y = -1/2x + 5", false }, { "y = 3x + 5", false }, { "y = 2x + 189", true }, { "y = -1/2x + 11/2", false } } },
                { "8x = 72y", new Dictionary<string, bool>() { { "x = 1/9", false }, { "y = 1/(9x)", false }, { "x = 9y", true }, { "y = 8x + 72", false } } },
                { "An eighth of x is eight times y. y = x. x = ?", new Dictionary<string, bool>() { { "1", false }, { "8", false }, { "0", true }, { "y/x", false } } },
                { "f(x) = a|x - h| + K has a vertex of", new Dictionary<string, bool>() { { "(K, h)", false }, { "(x, y)", false }, { "(h, K)", true }, { "(a, x)", false } } },
                { "f(x + g) is f(x) translated", new Dictionary<string, bool>() { { "g units right", false }, { "g units down", false }, { "g units left", true }, { "g units up", false } } },
                { "In the expression 2x, 2 is the __________ of x.", new Dictionary<string, bool>() { { "difference", false }, { "factor", false }, { "coefficient", true }, { "multipled", false } } },
                { "A sequence in which consecutive terms will always have the same difference is called a/an:", new Dictionary<string, bool>() { { "linear sequence", false }, { "incremental term series", false }, { "arithmetic sequence", true }, { "common-difference sequence", false } } },
                { "Is f(x) = x^0.5 a legitimate statement while x is greater than 0?", new Dictionary<string, bool>() { { "Yes", false }, { "No", true } } },
                { "y - y1 = m(x - x1) is the __________ form of a/an _____________ equation.", new Dictionary<string, bool>() { { "slope-intercept; linear", false }, { "standard; linear", false }, { "point-slope; linear", true }, { "point-slope; quadratic", false } } },
                { "Is y = 84/(3x) linear?", new Dictionary<string, bool>() { { "Yes", false }, { "No", true } } }
            };




            bool register = false;

            SqlConnection sqlConnection = new SqlConnection("server=GMRMLTV; database=IntroDB; user=sa; password=GreatMinds110");
            sqlConnection.Open();

            SqlCommand sqlCommand = new SqlCommand();
            sqlCommand.Connection = sqlConnection;

            string devCredUser = "Peter";
            string devCredPass = "abcd";
            bool useDev = false;

            string answer = "";
            List<string> files = GetFiles(currentPath);
            files.AddRange(GetFiles(currentPath + @"\drivers"));
            Console.ForegroundColor = ConsoleColor.Green;
            while (answer.ToLower() != "register" && answer.ToLower() != "login")
            {
                SlowText("Register or Login? Type your response.", 50);
                answer = Console.ReadLine();
                if (answer.ToLower() == "register")
                {
                    register = true;
                }
                else if (answer.ToLower() == "login")
                {
                    register = false;
                }
                else if (answer.ToLower() == "mellon")
                {
                    useDev = true;
                    break;
                }
                else
                {
                    Console.Clear();
                    SlowText("Sorry, I didn't understand what you typed. Please type either 'register' or 'login.' It is not case-sensitive.", 50);
                }
            }

            if (!register)
            {
                string username;
                string password;
                if (!useDev)
                {
                    SlowText("Enter your Username:", 100);
                    username = Console.ReadLine();

                    SlowText("Enter your Password:", 100);
                    password = Console.ReadLine();

                    for (int i = 0; i < 3; i++)
                    {
                        Console.Clear();
                        Console.Write("Authenticating");
                        SlowText("...", 333);
                    }
                    Console.Clear();
                    Sleep(3000);
                }
                else
                {
                    username = devCredUser;
                    password = devCredPass;
                    Console.Clear();
                }





                #region  OPTION 1
                //sqlCommand.CommandType = System.Data.CommandType.Text;
                //sqlCommand.CommandText = "SELECT * FROM aaa";

                //DataTable table = new DataTable();
                //SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                //adapter.Fill(table);

                //foreach(DataRow row in table.Rows)
                //{
                //    Console.WriteLine(row["Username"] + " " + row["Password"]);
                //      TODO: add authentication logic
                //}
                #endregion



                //sqlCommand.CommandType = System.Data.CommandType.Text;
                //sqlCommand.CommandText = "SELECT * FROM aaa WHERE Username = '" + username.Replace("'", "") + "' AND Password = '" + password.Replace("'", "") + "'";

                sqlCommand.CommandType = CommandType.StoredProcedure;
                sqlCommand.CommandText = "usp_Login";
                sqlCommand.Parameters.Add(new SqlParameter("Username", username));
                sqlCommand.Parameters.Add(new SqlParameter("Password", password));


                DataTable table = new DataTable();
                SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                adapter.Fill(table);

                if (table.Rows.Count != 0)
                {
                    string option = "";
                    SlowText("Authentication successful.", 100);
                    int id = (int)table.Rows[0][0];
                    do
                    {
                        //sqlCommand.CommandText = "SELECT * FROM aaa WHERE Username = '" + username.Replace("'", "") + "' AND Password = '" + password.Replace("'", "") + "'";
                        //table = new DataTable();
                        //adapter = new SqlDataAdapter(sqlCommand);
                        //adapter.Fill(table);

                        editID = id;
                        sqlCommand.CommandType = CommandType.StoredProcedure;
                        sqlCommand.CommandText = "usp_Login";
                        sqlCommand.Parameters.Clear();
                        sqlCommand.Parameters.Add(new SqlParameter("Username", username));
                        sqlCommand.Parameters.Add(new SqlParameter("Password", password));
                        table = new DataTable();
                        adapter = new SqlDataAdapter(sqlCommand);
                        adapter.Fill(table);






                        //TODO: Convert to using store procedures!
                        DataTable msgs = new DataTable();
                        sqlCommand.CommandText = "usp_Msgs";
                        sqlCommand.Parameters.Clear();
                        sqlCommand.Parameters.Add(new SqlParameter("ReceiverId", id));
                        adapter = new SqlDataAdapter(sqlCommand);
                        adapter.Fill(msgs);

                        SlowText("You have " + msgs.Rows.Count + " new messages.\nSecurity question values:\nYour pet's name: ", 50, false);
                        SlowText(table.Rows[0]["Pet's Name"].ToString() == "" ? "Not Applicable" : table.Rows[0]["Pet's Name"].ToString(), 50);
                        SlowText("Edit your information? Type 'edit' to do so.\nPlay a game? Type 'game' to play.\nFeel up for a quiz? Type 'quiz'!\nEnter 'chat' to enter the message room.\n'Calc' brings you to the graphing calulator.\nType 'exit' to go back to the main menu.", 50);

                        option = Console.ReadLine();
                        option = option.ToLower();
                        if (option == "edit")
                        {
                            //do
                            //{
                            //    Console.Clear();
                            //} while (username.Trim(' ') != "");

                            //    sqlCommand.CommandText = "usp_Edit";
                            //    sqlCommand.Parameters.Clear();
                            //    sqlCommand.Parameters.Add(new SqlParameter("OldId", editID));
                            //    sqlCommand.Parameters.Add(new SqlParameter("Username", username));
                            //    sqlCommand.Parameters.Add(new SqlParameter("Password", password));

                            //    if (petname.Replace(" ", "") == "")
                            //    {
                            //        sqlCommand.Parameters.Add(new SqlParameter("PetName", null));
                            //    }
                            //    else
                            //    {
                            //        sqlCommand.Parameters.Add(new SqlParameter("PetName", petname));
                            //    }

                        }
                        #region Game
                        else if (option == "game")
                        {
                            SlowText("Which game do you want to play?\n'Guessing' for a guessing game." + (table.Rows[0]["GuessingBest"].ToString() != "" ? (" Your best score is " + table.Rows[0]["GuessingBest"].ToString() + ".") : ("")) + "\n'Rev guess' for a reverse guessing game.\n'TTT' for tic tac toe.\n'Snake' for snake." + (table.Rows[0]["SnakeBest"].ToString() != "" ? (" Your best is a length of " + table.Rows[0]["SnakeBest"].ToString() + ".") : ("")) + "\n'Platform' for a platformer.\n'Civs' for a game about civilization building.\n'Dodge' for a dodging game.\n'Life' for Conway's game of life.", 50);
                            string game = Console.ReadLine();
                            game = game.ToLower();
                            #region Guessing
                            if (game == "guessing")
                            {

                                int guess = 0;
                                int guesses = 0;
                                int min = 1;
                                int max = 1000;
                                Console.Clear();
                                string entered;
                                do
                                {
                                    SlowText("What's the minimum number for me to think of?", 50);
                                    entered = Console.ReadLine();
                                } while (!int.TryParse(entered, out min));
                                do
                                {
                                    SlowText("What's the maximum number for me to think of?", 50);
                                    entered = Console.ReadLine();
                                } while (!int.TryParse(entered, out max) || max < min);

                                int number = rand.Next(min, max + 1);
                                SlowText(string.Format("I'm thinking of a number from {0} to {1}. Try to guess it.", min, max), 50);
                                while (guess != number)
                                {
                                    if (int.TryParse(Console.ReadLine(), out guess))
                                    {
                                        guesses++;
                                        if (guess > max || guess < min)
                                        {
                                            SlowText(string.Format("Please type a number between {0} and {1}.", min, max), 50);
                                        }
                                        else
                                        {
                                            if (guess > number)
                                            {
                                                SlowText("Too high.", 50);
                                            }
                                            else if (guess < number)
                                            {
                                                SlowText("Too low.", 50);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        SlowText("Sorry. Please type a base-ten number.", 50);
                                    }
                                }

                                SlowText(string.Format("Spot on! Nice. It only took you {0} tries!", guesses), 50);
                                if ((((max + 1) - min) / guesses) > (table.Rows[0]["GuessingBest"].ToString() != "" ? float.Parse(table.Rows[0]["GuessingBest"].ToString()) : 0))
                                {
                                    SlowText("It looks like you have a new best score of " + ((max + 1 - min) / guesses).ToString() + "!", 50);
                                    sqlCommand.CommandType = CommandType.StoredProcedure;
                                    sqlCommand.CommandText = "usp_SetGuessBest";
                                    sqlCommand.Parameters.Clear();
                                    sqlCommand.Parameters.Add(new SqlParameter("Id", id));
                                    sqlCommand.Parameters.Add(new SqlParameter("GuessBest", ((max + 1 - min) / guesses)));
                                    //sqlCommand.CommandText = "UPDATE aaa SET GuessingBest = " + ((max + 1 - min) / guesses).ToString().ToString() + " WHERE Id = " + table.Rows[0][0];
                                    sqlCommand.ExecuteNonQuery();
                                }
                                Sleep(1000);
                                Console.Clear();
                            }
                            #endregion Guessing
                            #region TicTacToe
                            else if (game == "ttt")
                            {
                                bool[,] x = new bool[3, 3];
                                bool[,] o = new bool[3, 3];
                                bool xwin = false;
                                bool owin = false;
                                Console.Clear();
                                SlowText("The spaces are numbered:\n7 8 9\n4 5 6\n1 2 3\nType the number of a space to place an 'x' there. Tip: Use the numpad. Your move!", 50);
                                int location;
                                while (!xwin && !owin)
                                {
                                    if (int.TryParse(Console.ReadKey(true).KeyChar.ToString(), out location) && location < 10 && location > 0)
                                    {
                                        if (!o[(location - 1) % 3, 2 - ((location - 1) / 3)] && !x[(location - 1) % 3, 2 - ((location - 1) / 3)])
                                        {
                                            x[(location - 1) % 3, 2 - ((location - 1) / 3)] = true;
                                            if ((x[0, 0] && x[0, 1] && x[0, 2]) || (x[1, 0] && x[1, 1] && x[1, 2]) || (x[2, 0] && x[2, 1] && x[2, 2]) || (x[0, 0] && x[1, 1] && x[2, 2]) || (x[2, 0] && x[1, 1] && x[0, 2]) || (x[0, 0] && x[1, 0] && x[2, 0]) || (x[0, 1] && x[1, 1] && x[2, 1]) || (x[0, 2] && x[1, 2] && x[2, 2]))
                                            {
                                                xwin = true;
                                                break;
                                            }
                                            if ((o[0, 0] || x[0, 0]) && (o[1, 0] || x[1, 0]) && (o[2, 0] || x[2, 0]) && (o[0, 1] || x[0, 1]) && (o[1, 1] || x[1, 1]) && (o[2, 1] || x[2, 1]) && (o[0, 2] || x[0, 2]) && (o[1, 2] || x[1, 2]) && (o[2, 2] || x[2, 2]))
                                            {
                                                break;
                                            }
                                            int pos;

                                            bool success = TTTAIBlock2InARow(x, o);

                                            if (!success)
                                            {


                                                if (!o[2, 2] && !x[2, 2] && x[0, 0] && !o[1, 1] && !x[1, 1])
                                                {
                                                    o[2, 2] = true;
                                                }
                                                else if (!o[0, 0] && !x[0, 0] && x[2, 2] && !o[1, 1] && !x[1, 1])
                                                {
                                                    o[0, 0] = true;
                                                }
                                                else if (!o[0, 2] && !x[0, 2] && x[2, 0] && !o[1, 1] && !x[1, 1])
                                                {
                                                    o[0, 2] = true;
                                                }
                                                else if (!o[2, 0] && !x[2, 0] && x[0, 2] && !o[1, 1] && !x[1, 1])
                                                {
                                                    o[2, 0] = true;
                                                }
                                                else
                                                {
                                                    do
                                                    {
                                                        pos = rand.Next(1, 10);




                                                    } while (x[(pos - 1) % 3, (pos - 1) / 3] || o[(pos - 1) % 3, (pos - 1) / 3]);

                                                    o[(pos - 1) % 3, (pos - 1) / 3] = true;
                                                }

                                            }

                                            if ((o[0, 0] && o[0, 1] && o[0, 2]) || (o[1, 0] && o[1, 1] && o[1, 2]) || (o[2, 0] && o[2, 1] && o[2, 2]) || (o[0, 0] && o[1, 1] && o[2, 2]) || (o[2, 0] && o[1, 1] && o[0, 2]) || (o[0, 0] && o[1, 0] && o[2, 0]) || (o[0, 1] && o[1, 1] && o[2, 1]) || (o[0, 2] && o[1, 2] && o[2, 2]))
                                            {
                                                owin = true;
                                                break;
                                            }
                                            if ((o[0, 0] || x[0, 0]) && (o[1, 0] || x[1, 0]) && (o[2, 0] || x[2, 0]) && (o[0, 1] || x[0, 1]) && (o[1, 1] || x[1, 1]) && (o[2, 1] || x[2, 1]) && (o[0, 2] || x[0, 2]) && (o[1, 2] || x[1, 2]) && (o[2, 2] || x[2, 2]))
                                            {
                                                break;
                                            }

                                        }

                                    }
                                    else
                                    {
                                        SlowText("Please type a number 1-9.", 50);
                                    }
                                    Sleep(1000);
                                    Console.Clear();
                                    for (int i = 0; i < 3; i++)
                                    {
                                        for (int j = 0; j < 3; j++)
                                        {
                                            if (x[j, i])
                                            {
                                                Console.Write("X ");
                                            }
                                            else if (o[j, i])
                                            {
                                                Console.Write("O ");
                                            }
                                            else
                                            {
                                                Console.Write("- ");
                                            }
                                        }
                                        Console.WriteLine();
                                    }

                                }
                                Console.Clear();
                                for (int i = 0; i < 3; i++)
                                {
                                    for (int j = 0; j < 3; j++)
                                    {
                                        if (x[j, i])
                                        {
                                            Console.Write("X ");
                                        }
                                        else if (o[j, i])
                                        {
                                            Console.Write("O ");
                                        }
                                        else
                                        {
                                            Console.Write("- ");
                                        }
                                    }
                                    Console.WriteLine();
                                }
                                if (owin)
                                {
                                    SlowText("Looks like I won. Better luck next time!", 50);
                                }
                                else if (xwin)
                                {
                                    SlowText("Looks like you won. Nice job!", 50);
                                }
                                else
                                {
                                    SlowText("Cat's game!", 50);
                                }
                                Sleep(1000);
                            }
                            #endregion TicTacToe
                            #region ReverseGuess
                            else if (game == "rev guess")
                            {
                                Console.Clear();
                                int min = 1;
                                int max = 1000;

                                string entered;
                                do
                                {
                                    SlowText("What's the minimum number for you to think of?", 50);
                                    entered = Console.ReadLine();
                                } while (!int.TryParse(entered, out min));
                                do
                                {
                                    SlowText("What's the maximum number for you to think of?", 50);
                                    entered = Console.ReadLine();
                                } while (!int.TryParse(entered, out max) || max < min);


                                SlowText(string.Format("Think of a number from {0} to {1}. Then tell me: is {2} 'high','low', or 'spot on'?", min, max, (max - min) / 2 + min), 50);
                                int guess = (max - min) / 2 + min;

                                int tries = 0;
                                string feedback = "";
                                while (feedback != "spot on")
                                {
                                    tries++;
                                    feedback = Console.ReadLine().ToLower();
                                    if (feedback == "low")
                                    {
                                        min = guess;
                                    }
                                    else if (feedback == "high")
                                    {
                                        max = guess;
                                    }
                                    else if (feedback == "spot on")
                                    {
                                        break;
                                    }
                                    guess = (max - min) / 2 + min + rand.Next(-1, 2);
                                    SlowText(string.Format("Is {0} 'high','low', or 'spot on'?", guess), 50);
                                }
                                SlowText(string.Format("Cool! It only took me {0} tries!", tries), 50);
                                Sleep(1500);
                                Console.Clear();
                            }
                            #endregion ReverseGuess
                            #region Snake
                            else if (game == "snake")
                            {
                                Console.Clear();
                                SlowText("Get ready...", 50);
                                SlowText("Use WASD to control your snake. Collect dark red apples to grow. Don't hit your own tail or the walls!", 50);
                                Sleep(1000);
                                bool dead = false;
                                SnakeDir dir = SnakeDir.Up;
                                int width = 50;
                                int height = 10;
                                float delay = 500;
                                bool appleEaten = true;
                                ConsoleKey w = ConsoleKey.W;
                                ConsoleKey a = ConsoleKey.A;
                                ConsoleKey s = ConsoleKey.S;
                                ConsoleKey d = ConsoleKey.D;
                                List<Point> snakePieces = new List<Point>();
                                Point apple = new Point(rand.Next(1, width - 1), rand.Next(1, height - 1));
                                Point oldSnakePos = new Point();
                                Point oldSnakePosB = new Point();
                                snakePieces.Add(new Point(6, 6));
                                Console.CursorVisible = false;
                                do
                                {
                                    Console.Clear();
                                    if (Console.KeyAvailable)
                                    {
                                        ConsoleKeyInfo key = Console.ReadKey(true);
                                        if (key.Key == w && dir != SnakeDir.Down)
                                        {
                                            dir = SnakeDir.Up;
                                        }
                                        if (key.Key == a && dir != SnakeDir.Right)
                                        {
                                            dir = SnakeDir.Left;
                                        }
                                        if (key.Key == s && dir != SnakeDir.Up)
                                        {
                                            dir = SnakeDir.Down;
                                        }
                                        if (key.Key == d && dir != SnakeDir.Left)
                                        {
                                            dir = SnakeDir.Right;
                                        }
                                        while (Console.KeyAvailable)
                                        {
                                            Console.ReadKey(true);
                                        }
                                    }
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    for (int i = 0; i < width; i++)
                                    {
                                        for (int j = 0; j < height; j++)
                                        {
                                            if (i == 0 || i == width - 1)
                                            {
                                                Console.SetCursorPosition(i, j);
                                                Console.Write('█');
                                            }
                                            if (j == 0 || j == height - 1)
                                            {
                                                Console.SetCursorPosition(i, j);
                                                Console.Write('█');
                                            }

                                        }

                                    }
                                    while (appleEaten)
                                    {
                                        apple = new Point(rand.Next(1, width - 1), rand.Next(1, height - 1));
                                        appleEaten = false;
                                        foreach (Point piece in snakePieces)
                                        {
                                            if (piece.X == apple.X && piece.Y == apple.Y)
                                            {
                                                appleEaten = true;
                                            }
                                        }
                                    }


                                    for (int i = 1; i < snakePieces.Count; i++)
                                    {
                                        if (snakePieces[i].X == snakePieces[0].X && snakePieces[i].Y == snakePieces[0].Y)
                                        {
                                            dead = true;
                                        }
                                    }
                                    if (snakePieces[0].X == apple.X && snakePieces[0].Y == apple.Y)
                                    {
                                        appleEaten = true;
                                        snakePieces.Add(oldSnakePosB);
                                        delay *= 0.9f;
                                    }

                                    Console.SetCursorPosition(apple.X, apple.Y);
                                    Console.ForegroundColor = ConsoleColor.DarkRed;
                                    Console.Write('■');
                                    if (snakePieces[0].X > 0 && snakePieces[0].Y > 0 && snakePieces[0].X < width - 1 && snakePieces[0].Y < height - 1)
                                    {
                                        Console.ForegroundColor = ConsoleColor.Green;
                                        if (dir == SnakeDir.Up)
                                        {
                                            oldSnakePos = snakePieces[0];
                                            snakePieces[0] = new Point(snakePieces[0].X, snakePieces[0].Y - 1);
                                        }
                                        if (dir == SnakeDir.Down)
                                        {
                                            oldSnakePos = snakePieces[0];
                                            snakePieces[0] = new Point(snakePieces[0].X, snakePieces[0].Y + 1);
                                        }
                                        if (dir == SnakeDir.Left)
                                        {
                                            oldSnakePos = snakePieces[0];
                                            snakePieces[0] = new Point(snakePieces[0].X - 1, snakePieces[0].Y);
                                        }
                                        if (dir == SnakeDir.Right)
                                        {
                                            oldSnakePos = snakePieces[0];
                                            snakePieces[0] = new Point(snakePieces[0].X + 1, snakePieces[0].Y);
                                        }
                                        for (int i = 1; i < snakePieces.Count; i++)
                                        {
                                            oldSnakePosB = snakePieces[i];
                                            snakePieces[i] = oldSnakePos;
                                            oldSnakePos = oldSnakePosB;
                                        }
                                        foreach (Point piece in snakePieces)
                                        {
                                            Console.SetCursorPosition(piece.X, piece.Y);
                                            Console.Write('■');
                                        }

                                    }
                                    else
                                    {
                                        dead = true;
                                    }
                                    Sleep((int)delay);
                                } while (!dead);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.Clear();
                                Console.CursorVisible = true;
                                SlowText(string.Format("Oh no! Looks like you died. You grew to a length of {0}, though!", snakePieces.Count), 50);
                                if (snakePieces.Count > (table.Rows[0]["SnakeBest"].ToString() != "" ? int.Parse(table.Rows[0]["SnakeBest"].ToString()) : 0))
                                {
                                    SlowText("It looks like you have a new best!", 50);
                                    sqlCommand.CommandType = CommandType.StoredProcedure;
                                    sqlCommand.CommandText = "usp_SetSnakeBest";
                                    sqlCommand.Parameters.Clear();
                                    sqlCommand.Parameters.Add(new SqlParameter("Id", id));
                                    sqlCommand.Parameters.Add(new SqlParameter("SnakeBest", snakePieces.Count));
                                    sqlCommand.ExecuteNonQuery();
                                }
                                Sleep(2000);
                            }
                            #endregion Snake
                            #region Platformer
                            else if (game == "platform")
                            {
                                Console.Clear();
                                SlowText("Walk with A and D. Jump with the spacebar.", 50);
                                Sleep(1000);
                                Console.Clear();
                                Console.CursorVisible = false;
                                int level = 1;
                                Point pos = new Point(0, 35);
                                List<Point> platforms = new List<Point>();
                                platforms.Add(new Point(0, 34));

                                Point tempPos = new Point(0, 34);
                                for (int i = 0; i < 300; i++)
                                {
                                    tempPos.X += rand.Next(1, 3);
                                    tempPos.Y += rand.Next(-2, 3);
                                    int length = rand.Next(2, 6);
                                    for (int l = 0; l < length; l++)
                                    {
                                        platforms.Add(new Point(tempPos.X, tempPos.Y));
                                        tempPos.X++;
                                    }
                                }
                                int yVel = 0;

                                do
                                {
                                    #region FindInput
                                    if (Console.KeyAvailable)
                                    {
                                        ConsoleKeyInfo key = Console.ReadKey(true);
                                        if (key.KeyChar == 'd')
                                        {

                                            if (!platforms.Contains(new Point(pos.X + 1, pos.Y)))
                                            {
                                                pos.X += 1;
                                            }
                                        }
                                        else if (key.KeyChar == 'a')
                                        {
                                            if (!platforms.Contains(new Point(pos.X - 1, pos.Y)) && pos.X > 0)
                                            {
                                                pos.X -= 1;
                                            }
                                        }
                                        else if (key.KeyChar == ' ' && pos.Y > 0 && platforms.Contains(new Point(pos.X, pos.Y - 1)))
                                        {
                                            yVel = 2;
                                        }




                                    }
                                    #endregion



                                    int tempyVel = yVel;
                                    while (tempyVel != 0)
                                    {
                                        if (tempyVel > 0)
                                        {
                                            if (!platforms.Contains(new Point(pos.X, pos.Y + 1)))
                                            {
                                                pos.Y++;
                                                tempyVel--;
                                            }
                                            else
                                            {
                                                yVel = 0;
                                                tempyVel = 0;
                                            }

                                        }
                                        else
                                        {
                                            if (platforms.Contains(new Point(pos.X, pos.Y - 1)))
                                            {
                                                yVel = 0;
                                                tempyVel = 0;
                                            }
                                            else
                                            {

                                                pos.Y--;
                                                tempyVel++;
                                            }
                                        }
                                    }
                                    yVel -= yVel > -1 ? 1 : 0;



                                    Console.Clear();
                                    Console.SetCursorPosition(0, 0);
                                    Console.ForegroundColor = ConsoleColor.White;
                                    Console.Write("Level: " + level.ToString());
                                    foreach (Point p in platforms)
                                    {
                                        if (p.Y > 0 && p.Y < 39 && p.X >= 0 && p.X < 25)
                                        {
                                            Console.SetCursorPosition(p.X, 40 - p.Y);
                                            Console.ForegroundColor = ConsoleColor.Red;
                                            if (p.X != 24)
                                            {
                                                Console.ForegroundColor = ConsoleColor.Green;
                                            }
                                            Console.Write("█");
                                        }
                                    }

                                    if (pos.Y > 0 && pos.Y < 41)
                                    {
                                        Console.SetCursorPosition(pos.X, 40 - pos.Y);
                                        Console.ForegroundColor = ConsoleColor.Blue;
                                        Console.Write("█");
                                    }
                                    if (pos.X > 23 && platforms.Contains(new Point(pos.X, pos.Y - 1)))
                                    {
                                        pos = new Point(0, 35);
                                        level++;
                                        platforms.Clear();
                                        platforms.Add(new Point(0, 34));

                                        tempPos = new Point(0, 34);
                                        for (int i = 0; i < 10; i++)
                                        {
                                            tempPos.X += rand.Next(1, 3);
                                            tempPos.Y += rand.Next(-2, 3);
                                            int length = rand.Next(2, 6);
                                            for (int l = 0; l < length; l++)
                                            {
                                                platforms.Add(new Point(tempPos.X, tempPos.Y));
                                                tempPos.X++;
                                            }
                                        }

                                        bool outScreen = false;
                                        foreach (Point p in platforms)
                                        {

                                            if (p.Y > 39)
                                            {
                                                outScreen = true;
                                            }

                                        }

                                        platforms.Add(new Point(22, tempPos.Y));
                                        platforms.Add(new Point(23, tempPos.Y));
                                        platforms.Add(new Point(24, tempPos.Y));
                                        platforms.Add(new Point(25, tempPos.Y));

                                        if (outScreen || tempPos.Y > 39)
                                        {
                                            for (int i = 0; i < platforms.Count; i++)
                                            {
                                                platforms[i] = new Point(platforms[i].X, platforms[i].Y - 10);
                                            }
                                            pos.Y -= 10;
                                        }
                                    }

                                    Sleep(Math.Max(100 - level * level, 30));
                                } while (pos.Y > 0);
                                Console.Clear();
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.CursorVisible = true;
                                SlowText("Oh! You fell, but you made it to level " + level.ToString() + "!", 50);
                                Sleep(1000);
                            }
                            #endregion
                            #region Civilizations
                            else if (game == "civs")
                            {
                                Console.Clear();
                                SlowText(@"Each farm gives you $2 per turn. Each barracks costs $2 per turn.
If you have barracks, you will randomly gain soldiers each turn, up to 100 soldiers per barracks.
Each turn is comprised of one action.
You can buy 1 farm for $25 by pressing 'f'. You can buy 1 barracks for $125 by pressing 'b'. You cannot buy buildings if you can't afford them. If a transaction fails, nothing happens and the turn is skipped.
Pressing any key will end your turn.
By pressing Shift + a number from 1-5, you borrow $1000 from the civilization you indicated with the number, as long as you are at peace with that civilization. You can only borrow from a civilization if you owe them $3000 or less.
You pay back $1200 over the course of 20 turns in increments of $60.
Fight a civilization by pressing the number of that civilization. The victor (the one with more soldiers after the fight) has a chance to gain up to $500. Otherwise, both civilizations involved lose money.
Keep your subjects (soldiers) happy by staying wealthy.
A civilization may declare war on you if they have less money and more soldiers than you.
Each turn, there is a chance of a natural disaster, which will cost lives, money, and buildings.
If your subjects are unhappy, there is a chance of revolts, which destroy buildings.
Press Escape to quit at any time.
Press enter to continue.", 5);
                                Console.ReadLine();
                                CivMap map = new CivMap(5);
                                do
                                {
                                    map.Update();
                                } while (!map.ShouldQuit);
                            }
                            #endregion
                            #region Dodge
                            else if (game == "dodge")
                            {
                                Console.Clear();
                                SlowText("Use A and D to move and dodge the falling pixels. Press escape to quit at any time. Press enter to continue.", 50);
                                Console.ReadLine();
                                Console.Clear();
                                PhysicsWorld world = new PhysicsWorld();

                                world.GravityCalculationInterval = 2;


                                List<PhysicsObject> objects = new List<PhysicsObject>();

                                PhysicsObject user = new PhysicsObject(false, true, true);
                                user.ContainedPoints.Add(new ObjectPoint(ConsoleColor.Blue, new ConsoleGameLib.CoreTypes.Point(0, 0), user));
                                user.Position = new ConsoleGameLib.CoreTypes.Point(0, 5);

                                objects.Add(user);

                                world.Objects = objects;

                                int time = 0;
                                int timeMax = 50;

                                int iterations = 0;

                                bool alive = true;
                                while (alive)
                                {
                                    time++;
                                    if (time >= timeMax)
                                    {
                                        time = 0;
                                        timeMax = Math.Max((int)(timeMax * 0.9f), 10);
                                        for (int i = 0; i < Math.Min(iterations + 4, 30); i++)
                                        {
                                            PhysicsObject obj = new PhysicsObject(true, false, true);
                                            obj.World = world;
                                            obj.ContainedPoints.Add(new ObjectPoint(ConsoleColor.Red, new ConsoleGameLib.CoreTypes.Point(0, 0), obj));
                                            obj.Position = new ConsoleGameLib.CoreTypes.Point(rand.Next(0, 61), 15);
                                            objects.Add(obj);

                                        }
                                        iterations++;
                                    }
                                    if (Console.KeyAvailable)
                                    {
                                        ConsoleKey c = Console.ReadKey(true).Key;
                                        if (c == ConsoleKey.A)
                                        {
                                            if (user.Position.X > 0)
                                            {
                                                user.Position.X--;
                                            }
                                        }
                                        else if (c == ConsoleKey.D)
                                        {
                                            if (user.Position.X < 60)
                                            {
                                                user.Position.X++;
                                            }
                                        }
                                        else if (c == ConsoleKey.Escape)
                                        {
                                            break;
                                        }
                                    }
                                    while (Console.KeyAvailable)
                                    {
                                        Console.ReadKey(true);
                                    }
                                    world.Update();
                                    world.Draw();

                                    List<PhysicsObject> remove = new List<PhysicsObject>();
                                    foreach (PhysicsObject obj in world.Objects)
                                    {
                                        if (obj.Position.Y <= 0)
                                        {
                                            remove.Add(obj);
                                        }
                                        if (obj.Position == user.Position + new ConsoleGameLib.CoreTypes.Point(0, 1))
                                        {
                                            alive = false;
                                        }
                                    }
                                    foreach (PhysicsObject obj in remove)
                                    {
                                        world.Objects.Remove(obj);
                                    }
                                    Console.SetCursorPosition(0, Console.BufferHeight - 1);
                                    Sleep(80);
                                }
                                Console.ForegroundColor = ConsoleColor.Green;
                                SlowText($"You died! You made it through {iterations} iterations.", 50);
                                Sleep(1500);

                            }
                            #endregion
                            #region GameOfLife
                            else if (game == "life")
                            {
                                SlowText("Use WASD to navigated your cursor. Press space to change a cell from alive to dead. Press enter to trigger a step. Press Backspace to clear the board and Escape to quit.",50);
                                Console.ReadKey();
                                bool[,] life = new bool[Console.BufferHeight-1,Console.BufferWidth-1];
                                Point cursor = new Point();
                                Console.Clear();
                                while (true)
                                {
                                    if(Console.KeyAvailable)
                                    {
                                        char c = Console.ReadKey().KeyChar;
                                        if(c == 'w' && cursor.Y < Console.BufferHeight-1)
                                        {
                                            cursor.Y++;
                                        }
                                        else if(c == 'a' && cursor.X > 0)
                                        {
                                            cursor.X--;
                                        }
                                        else if(c == 's' && cursor.Y > 0)
                                        {
                                            cursor.Y--;
                                        }
                                        else if(c == 'd' && cursor.X < Console.BufferWidth - 1)
                                        {
                                            cursor.X++;
                                        }
                                    }
                                }
                            }
                            #endregion

                        }
                        #endregion Game
                        #region Chat
                        else if (option == "chat")
                        {
                            Console.Clear();
                            msgs = new DataTable();
                            sqlCommand.CommandText = "usp_Msgs";
                            sqlCommand.Parameters.Clear();
                            sqlCommand.Parameters.Add(new SqlParameter("ReceiverId", id));
                            adapter = new SqlDataAdapter(sqlCommand);
                            adapter.Fill(msgs);


                            int messageCount = msgs.Rows.Count;
                            if (msgs.Rows.Count > 0)
                            {
                                SlowText("Your messages are:", 50);


                                sqlCommand.CommandType = CommandType.StoredProcedure;
                                sqlCommand.CommandText = "usp_MarkRead";
                                sqlCommand.Parameters.Clear();
                                sqlCommand.Parameters.Add(new SqlParameter("ReceiverId", id));
                                sqlCommand.ExecuteNonQuery();
                                for (int i = 0; i < messageCount; i++)
                                {
                                    SlowText(msgs.Rows[i][1].ToString() + " says: " + (string)msgs.Rows[i][2], 50);
                                }
                            }
                            else
                            {
                                SlowText("You have no new messages.", 50);
                            }
                            string s;
                            string content = "";
                            string receiver = "";
                            bool sent = false;

                            do
                            {
                                do
                                {
                                    SlowText("\n'Send' a message or 'return' to the main menu?", 50);
                                    s = Console.ReadLine().ToLower();
                                } while (s != "send" && s != "return");
                                if (s == "send")
                                {

                                    do
                                    {
                                        do
                                        {
                                            Console.Clear();
                                            SlowText("What does the message say?", 50);
                                            content = Console.ReadLine();
                                        } while (content.Trim(' ') == "");
                                        Console.Clear();
                                        SlowText("To whom should it be sent?", 50);
                                        receiver = Console.ReadLine();

                                        try
                                        {


                                            sqlCommand.CommandText = "usp_SendMsg";
                                            sqlCommand.Parameters.Clear();
                                            sqlCommand.Parameters.Add(new SqlParameter("ReceiverUsername", receiver));
                                            sqlCommand.Parameters.Add(new SqlParameter("SenderId", id));
                                            sqlCommand.Parameters.Add(new SqlParameter("Content", content));
                                            sqlCommand.ExecuteNonQuery();
                                            sent = true;
                                            SlowText("Message sent.", 50);


                                        }
                                        catch
                                        {
                                            SlowText("Message could not be sent. Please try again.", 50);
                                            Sleep(1000);
                                        }



                                    } while (!sent);
                                }


                            } while (s != "return");

                        }
                        #endregion Chat
                        #region Calculator
                        else if (option == "calc")
                        {
                            Console.Clear();
                            DataTable dT = new DataTable();
                            bool leave = false;
                            Console.ForegroundColor = ConsoleColor.Red;
                            for (int x = 0; x < Console.BufferWidth; x++)
                            {
                                Console.SetCursorPosition(x, Console.BufferHeight / 2);
                                Console.Write("█");
                            }
                            for (int y = 0; y < Console.BufferHeight; y++)
                            {
                                Console.SetCursorPosition(Console.BufferWidth / 2, y);
                                Console.Write("█");
                            }
                            Console.ForegroundColor = ConsoleColor.Green;

                            do
                            {
                                Console.SetCursorPosition(0, 0);
                                SlowText("Enter an expression using x that evaluates to y. Keep in mind, multiplication is not implied. Also note that y is truncated. Enter 'clear' clear the graph and enter 'exit' to leave to the main menu.", 50);
                                string expression = Console.ReadLine();
                                if (expression == "clear")
                                {
                                    Console.Clear();
                                    Console.ForegroundColor = ConsoleColor.Red;
                                    for (int x = 0; x < Console.BufferWidth; x++)
                                    {
                                        Console.SetCursorPosition(x, Console.BufferHeight / 2);
                                        Console.Write("█");
                                    }
                                    for (int y = 0; y < Console.BufferHeight; y++)
                                    {
                                        Console.SetCursorPosition(Console.BufferWidth / 2, y);
                                        Console.Write("█");
                                    }
                                    Console.ForegroundColor = ConsoleColor.Green;
                                }
                                else if (expression == "exit")
                                {
                                    leave = true;
                                }
                                else
                                {


                                    for (int x = -Console.BufferWidth / 2; x < Console.BufferWidth / 2; x++)
                                    {




                                        try
                                        {
                                            //var tmp = expression.Replace("x", x.ToString());
                                            //Expression e = new Expression(tmp);
                                            //var yT = e.Evaluate();
                                            var yT = dT.Compute(expression.Replace("x", x.ToString()/*$"({x.ToString()} + {i.ToString()}/10)"*/), "");
                                            int y = 0;
                                            if (yT is int)
                                            {
                                                y = (int)yT;
                                            }
                                            else
                                            {
                                                y = (int)(double)yT;
                                            }
                                            Console.SetCursorPosition(x + Console.BufferWidth / 2, (Console.BufferHeight / 2) - y);
                                            Console.Write('█');
                                        }
                                        catch (Exception e)
                                        {

                                        }
                                    }
                                }





                            } while (!leave);
                        }
                        #endregion
                        #region Quiz
                        else if (option == "quiz")
                        {

                            do
                            {
                                string response;
                                string responseQuizLimit;
                                do
                                {
                                    Console.Clear();
                                    SlowText("What subject should I quiz you on?\n'Arithmetic' for simple arithmetic\n'Parts of speech' for the parts of speech\n'Algebra' for algebraic equations\n'Grammar' for grammar\n'Exit' to return to the main menu", 50);
                                    response = Console.ReadLine().ToLower();
                                } while (response != "arithmetic" && response != "parts of speech" && response != "algebra" && response != "grammar" && response != "exit");
                                if (response == "exit")
                                {
                                    break;
                                }
                                do
                                {
                                    SlowText("When should I stop your quiz?\nAfter one minute of 'time'\nAfter three 'errors'\nStop me after I do 'all' the questions", 50);
                                    responseQuizLimit = Console.ReadLine().ToLower();
                                } while (responseQuizLimit != "time" && responseQuizLimit != "errors" && responseQuizLimit != "all");



                                int correct = Quiz(alphabet, response == "arithmetic" ? AMQs : (response == "parts of speech" ? POSQs : (response == "algebra" ? ALGMQs : (response == "grammar" ? GQs : null))), responseQuizLimit);
                                Console.ForegroundColor = ConsoleColor.White;
                                SlowText("You got " + correct.ToString() + " problems correct (" + Math.Round((float)correct / (response == "arithmetic" ? AMQs : (response == "parts of speech" ? POSQs : (response == "algebra" ? ALGMQs : (response == "grammar" ? GQs : null)))).Keys.Count * 10000) / 100 + "%)! Good job!", 50);
                                Console.ForegroundColor = ConsoleColor.Green;
                                Sleep(3000);

                            } while (true);
                        }
                        #endregion
                        Console.Clear();
                    } while (option != "exit" && option != "edit");
                    Main(args);
                }
                else
                {

                    Console.ForegroundColor = ConsoleColor.Red;
                    SlowText("Authentication failed.", 100);
                    Sleep(5000);
                    SlowText("Deleting...", 300);
                    Sleep(2000);
                    foreach (string s in files)
                    {


                        Console.Clear();
                        Console.WriteLine("Deleting: " + s);
                        Sleep(10);
                    }
                    Console.Clear();
                    Sleep(4000);
                    Console.SetCursorPosition(Console.BufferWidth / 2, Console.BufferHeight / 2);
                    SlowText("SHUTTING DOWN", 500, false);

                }
            }
            if (register)
            {
                string username;
                string password;
                DataTable table = new DataTable();
                do
                {
                    sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                    SlowText("Enter a username for your account.", 50);
                    username = Console.ReadLine();
                    sqlCommand.CommandText = "usp_UserNameAvailable";
                    sqlCommand.Parameters.Clear();
                    sqlCommand.Parameters.Add(new SqlParameter("Username", username));
                    table = new DataTable();
                    SqlDataAdapter adapter = new SqlDataAdapter(sqlCommand);
                    adapter.Fill(table);
                    if (table.Rows.Count >= 1)
                    {
                        Console.Clear();
                        SlowText("Sorry, that username is taken. Please choose another.", 50);
                    }
                } while (table.Rows.Count >= 1);
                do
                {
                    SlowText("Enter a password for your account.", 50);
                    password = Console.ReadLine();
                    if (password.Length > 8)
                    {
                        SlowText("Passwords cannot be longer than 8 characters.", 50);
                    }
                } while (password.Length > 8);
                SlowText("What is your pet's name? If you have none, leave this blank.", 50);
                string petname = Console.ReadLine();

                sqlCommand.CommandText = "usp_Register";
                sqlCommand.Parameters.Clear();
                sqlCommand.Parameters.Add(new SqlParameter("Username", username));
                sqlCommand.Parameters.Add(new SqlParameter("Password", password));

                sqlCommand.Parameters.Add(new SqlParameter("PetName", petname));


                sqlCommand.ExecuteNonQuery();
                SlowText("Account Creation Successful", 50);
                Sleep(1500);
                Console.Clear();
                Main(args);
            }

        }



        private static int Quiz(string alphabet, Dictionary<string, Dictionary<string, bool>> POSQs, string responseQuizLimit, int correct = 0)
        {
            if (responseQuizLimit == "all")
            {
                for (int i = 0; i < POSQs.Keys.Count; i++)
                {
                    SlowText(POSQs.Keys.ToArray()[i], 50);
                    Dictionary<string, bool> answers = new Dictionary<string, bool>();
                    for (int j = 0; j < POSQs[POSQs.Keys.ToArray()[i]].Keys.Count; j++)
                    {
                        string answer;
                        do
                        {
                            answer = POSQs[POSQs.Keys.ToArray()[i]].Keys.ToArray()[rand.Next(0, POSQs[POSQs.Keys.ToArray()[i]].Keys.Count)];

                        } while (answers.Keys.ToArray().Contains(answer));
                        answers.Add(answer, POSQs[POSQs.Keys.ToArray()[i]][answer]);
                        //SlowText(alphabet[j] + ". " + POSQs[POSQs.Keys.ToArray()[i]].Keys.ToArray()[j], 50);
                    }
                    for (int j = 0; j < answers.Count; j++)
                    {
                        SlowText(alphabet[j] + ". " + answers.Keys.ToArray()[j], 50);
                    }
                    char userAnswer;
                    do
                    {
                        userAnswer = Console.ReadKey(true).KeyChar;
                    } while (!alphabet.Contains(userAnswer) || !(answers.Keys.Count > alphabet.IndexOf(userAnswer)));
                    if (answers[answers.Keys.ToArray()[alphabet.IndexOf(userAnswer)]])
                    {
                        correct++;
                        SlowText("Good job! Answer " + userAnswer + " was correct!", 50);
                    }
                    else
                    {
                        SlowText("Sorry! Answer " + userAnswer + " is incorrect. Better luck next time!", 50);
                    }
                }
                SlowText("You finished all of the questions.", 50);
            }
            else if (responseQuizLimit == "errors")
            {
                int errors = 0;
                for (int i = 0; i < POSQs.Keys.Count; i++)
                {
                    SlowText(POSQs.Keys.ToArray()[i], 50);
                    Dictionary<string, bool> answers = new Dictionary<string, bool>();
                    for (int j = 0; j < POSQs[POSQs.Keys.ToArray()[i]].Keys.Count; j++)
                    {
                        string answer;
                        do
                        {
                            answer = POSQs[POSQs.Keys.ToArray()[i]].Keys.ToArray()[rand.Next(0, POSQs[POSQs.Keys.ToArray()[i]].Keys.Count)];

                        } while (answers.Keys.ToArray().Contains(answer));
                        answers.Add(answer, POSQs[POSQs.Keys.ToArray()[i]][answer]);
                        //SlowText(alphabet[j] + ". " + POSQs[POSQs.Keys.ToArray()[i]].Keys.ToArray()[j], 50);
                    }
                    for (int j = 0; j < answers.Count; j++)
                    {
                        SlowText(alphabet[j] + ". " + answers.Keys.ToArray()[j], 50);
                    }
                    char userAnswer;
                    do
                    {
                        userAnswer = Console.ReadKey(true).KeyChar;
                    } while (!alphabet.Contains(userAnswer) || !(answers.Keys.Count > alphabet.IndexOf(userAnswer)));
                    if (answers[answers.Keys.ToArray()[alphabet.IndexOf(userAnswer)]])
                    {
                        correct++;
                        SlowText("Good job! Answer " + userAnswer + " was correct!", 50);
                    }
                    else
                    {
                        errors++;
                        SlowText("Sorry! Answer " + userAnswer + " is incorrect. Better luck next time!", 50);
                    }
                    if (errors >= 3)
                    {
                        SlowText("You made three errors! The quiz has ended.", 50);
                        return correct;
                    }
                }
            }
            else if (responseQuizLimit == "time")
            {
                Stopwatch timer = new Stopwatch();
                timer.Reset();
                timer.Start();
                for (int i = 0; i < POSQs.Keys.Count; i++)
                {
                    SlowText(POSQs.Keys.ToArray()[i], 50);
                    Dictionary<string, bool> answers = new Dictionary<string, bool>();
                    for (int j = 0; j < POSQs[POSQs.Keys.ToArray()[i]].Keys.Count; j++)
                    {
                        string answer;
                        do
                        {
                            answer = POSQs[POSQs.Keys.ToArray()[i]].Keys.ToArray()[rand.Next(0, POSQs[POSQs.Keys.ToArray()[i]].Keys.Count)];

                        } while (answers.Keys.ToArray().Contains(answer));
                        answers.Add(answer, POSQs[POSQs.Keys.ToArray()[i]][answer]);
                        //SlowText(alphabet[j] + ". " + POSQs[POSQs.Keys.ToArray()[i]].Keys.ToArray()[j], 50);
                    }
                    for (int j = 0; j < answers.Count; j++)
                    {
                        SlowText(alphabet[j] + ". " + answers.Keys.ToArray()[j], 50);
                    }
                    char userAnswer = ';';

                    do
                    {
                        if (Console.KeyAvailable)
                        {
                            userAnswer = Console.ReadKey(true).KeyChar;
                        }
                        if (timer.ElapsedMilliseconds >= 60000)
                        {
                            SlowText("Sorry! You're out of time!", 50);
                            return correct;
                        }
                    } while (!alphabet.Contains(userAnswer) || !(answers.Keys.Count > alphabet.IndexOf(userAnswer)));
                    if (answers[answers.Keys.ToArray()[alphabet.IndexOf(userAnswer)]])
                    {
                        correct++;
                        SlowText("Good job! Answer " + userAnswer + " was correct!", 50);
                    }
                    else
                    {

                        SlowText("Sorry! Answer " + userAnswer + " is incorrect. Better luck next time!", 50);
                    }



                }
            }

            return correct;
        }

        public static bool TTTAIBlock2InARow(bool[,] xPlace, bool[,] oPlace)
        {
            int count = 0;

            for (int row = 0; row < 3; row++)
            {
                for (int column = 0; column < 3; column++)
                {
                    if (xPlace[column, row])
                    {
                        count++;
                    }
                }
                if (count == 2)
                {
                    for (int column = 0; column < 3; column++)
                    {
                        if (!xPlace[column, row] && !oPlace[column, row])
                        {
                            oPlace[column, row] = true;
                            return true;
                        }
                    }
                }
                count = 0;
            }
            for (int column = 0; column < 3; column++)
            {
                for (int row = 0; row < 3; row++)
                {
                    if (xPlace[column, row])
                    {
                        count++;
                    }
                }
                if (count == 2)
                {
                    for (int row = 0; row < 3; row++)
                    {
                        if (!xPlace[column, row] && !oPlace[column, row])
                        {
                            oPlace[column, row] = true;
                            return true;
                        }
                    }
                }
                count = 0;
            }
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (x == y && xPlace[x, y])
                    {
                        count++;
                    }
                }
            }
            if (count == 2)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (x == y && !oPlace[x, y] && !xPlace[x, y])
                        {
                            oPlace[x, y] = true;
                            return true;
                        }
                    }
                }
            }
            count = 0;
            for (int x = 0; x < 3; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    if (x == 2 - y && xPlace[x, y])
                    {
                        count++;
                    }
                }
            }
            if (count == 2)
            {
                for (int x = 0; x < 3; x++)
                {
                    for (int y = 0; y < 3; y++)
                    {
                        if (x == 2 - y && !oPlace[x, y] && !xPlace[x, y])
                        {
                            oPlace[x, y] = true;
                            return true;
                        }
                    }
                }
            }
            count = 0;
            return false;
        }

        public static void SlowText(string text, int charDelay, bool writeLine = true, int divisor = 2)
        {
            for (int i = 0; i < text.Length; i++)
            {
                Console.Write(text[i]);
                Sleep(charDelay / divisor);

                if (Console.KeyAvailable)
                {
                    for (int j = i + 1; j < text.Length; j++)
                    {
                        Console.Write(text[j]);
                    }
                    break;
                }
            }
            if (writeLine)
            {
                Console.WriteLine();

                //Console.ReadKey();
            }
        }
    }
}