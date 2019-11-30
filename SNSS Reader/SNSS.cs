using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SNSS_Reader
{
    public class SNSS
    {
        public struct Command
        {
            public byte Id;
            public object Content;

            public Command(byte[] data)
            {
                Id = data[0];

                byte[] content = new byte[data.Length - 1];
                Array.Copy(data, 1, content, 0, content.Length);

                if (Id == 1 || Id == 6)
                    Content = new Tab(content);
                else
                    Content = content;
            }

            public override string ToString()
            {
                if (Content is Tab)               
                    return ((Tab)Content).ToString();                
                else
                    return SNSS.ToString((byte[])Content);
            }
        }

        public struct Tab
        {
            public int Id;
            public int Index;                   //in this tab’s back-forward list
            public string URL;                  //ASCII
            public string Title;                //UTF-16
            public TabState State;
            public int TransitionType;
            public int POST;                    //1 if the page has POST data, otherwise 0
            public string ReferrerURL;          //ASCII
            public int ReferencePolicy;
            public string OriginalRequestURL;   //ASCII
            public int UserAgent;               //1 if the user-agent was overridden, otherwise 0

            public Tab(byte[] data)
            {
                int urlLength = BitConverter.ToInt32(data, 12);
                int titleOffset = urlLength % 4 == 0 ? urlLength + 16 : urlLength / 4 * 4 + 20;
                int titleLength = BitConverter.ToInt32(data, titleOffset) * 2;
                int stateOffset = titleLength % 4 == 0 ? titleLength + titleOffset + 4 : titleLength / 4 * 4 + titleOffset + 8;
                int stateLength = BitConverter.ToInt32(data, stateOffset);
                int transitionTypeOffset = stateLength % 4 == 0 ? stateLength + stateOffset + 4 : stateLength / 4 * 4 + stateOffset + 8;
                int refURLLength = BitConverter.ToInt32(data, transitionTypeOffset + 8);
                int refPolicyOffset = refURLLength % 4 == 0 ? refURLLength + transitionTypeOffset + 12 : refURLLength / 4 * 4 + transitionTypeOffset + 16;
                int reqURLLength = BitConverter.ToInt32(data, refPolicyOffset + 4);
                int userAgentOffset = reqURLLength % 4 == 0 ? reqURLLength + refPolicyOffset + 8 : reqURLLength / 4 * 4 + refPolicyOffset + 12;

                Id = BitConverter.ToInt32(data, 4);
                Index = BitConverter.ToInt32(data, 8);
                URL = Encoding.ASCII.GetString(data, 16, urlLength);
                Title = Encoding.Unicode.GetString(data, titleOffset + 4, titleLength);
                byte[] state = new byte[stateLength];
                Array.Copy(data, stateOffset + 4, state, 0, state.Length);
                State = new TabState(state);
                TransitionType = BitConverter.ToInt32(data, transitionTypeOffset);
                POST = BitConverter.ToInt32(data, transitionTypeOffset + 4);
                ReferrerURL = Encoding.ASCII.GetString(data, transitionTypeOffset + 12, refURLLength);
                ReferencePolicy = BitConverter.ToInt32(data, refPolicyOffset);
                OriginalRequestURL = Encoding.ASCII.GetString(data, refPolicyOffset + 8, reqURLLength);
                UserAgent = BitConverter.ToInt32(data, userAgentOffset);
            }

            public override string ToString()
            {
                StringBuilder strBuilder = new StringBuilder();

                strBuilder.AppendLine("Id: " + Id.ToString());
                strBuilder.AppendLine("Index: " + Index.ToString());
                strBuilder.AppendLine("URL: " + URL);
                strBuilder.AppendLine("Title: " + Title);

                strBuilder.AppendLine("Transition type: 0x" + TransitionType.ToString("X8"));
                switch (TransitionType & 0xFF)
                {
                    case 0:
                        strBuilder.AppendLine("  User arrived at this page by clicking a link on another page.");
                        break;
                    case 1:
                        strBuilder.AppendLine("  User typed URL into the Omnibar, or clicked a suggested URL in the Omnibar.");
                        break;
                    case 2:
                        strBuilder.AppendLine("  User arrived at page through a  bookmark or similar (eg. \"most visited\" suggestions on a new tab).");
                        break;
                    case 3:
                        strBuilder.AppendLine("  Automatic navigation within a sub frame (eg an embedded ad).");
                        break;
                    case 4:
                        strBuilder.AppendLine("  Manual navigation in a sub frame.");
                        break;
                    case 5:
                        strBuilder.AppendLine("  User selected suggestion from Omnibar (ie. typed part of an address or search term then selected a suggestion which was not a URL).");
                        break;
                    case 6:
                        strBuilder.AppendLine("  Start page (or specified as a command line argument).");
                        break;
                    case 7:
                        strBuilder.AppendLine("  User arrived at this page as a result of submitting a form.");
                        break;
                    case 8:
                        strBuilder.AppendLine("  Page was reloaded; either by clicking the refresh button, hitting F5, hitting enter in the address bar or as result of restoring a previous session.");
                        break;
                    case 9:
                        strBuilder.AppendLine("  Generated as a result of a keyword search, not using the default search provider (for example using tab-to-search on Wikipedia).");
                        break;
                    /*case 10:
                        strBuilder.AppendLine("  10.");
                        break;*/
                    default:
                        strBuilder.AppendLine("  " + (TransitionType & 0xFF).ToString() + ".");
                        break;
                }
                if ((TransitionType & 0x01000000) == 0x01000000)
                    strBuilder.AppendLine("  User used the back or forward buttons to arrive at this page.");
                if ((TransitionType & 0x02000000) == 0x02000000)
                    strBuilder.AppendLine("	 User used the address bar to trigger this navigation.");
                if ((TransitionType & 0x04000000) == 0x04000000)
                    strBuilder.AppendLine("  User is navigating to the homepage.");
                if ((TransitionType & 0x10000000) == 0x10000000)
                    strBuilder.AppendLine("  The beginning of a navigation chain.");
                if ((TransitionType & 0x20000000) == 0x20000000)
                    strBuilder.AppendLine("  Last transition in a redirect chain.");
                if ((TransitionType & 0x40000000) == 0x40000000)
                    strBuilder.AppendLine("  Transition was a client-side redirect (eg. caused by JavaScript or a meta-tag redirect).");
                if ((TransitionType & 0x80000000) == 0x08000000)
                    strBuilder.AppendLine("  Transition was a server-side redirect (ie a redirect specified in the HTTP response header).");

                if (POST == 0)
                    strBuilder.AppendLine("The page has no POST data.");
                else if (POST == 1)
                    strBuilder.AppendLine("The page has POST data.");
                else
                    strBuilder.AppendLine("POST: " + POST.ToString());

                strBuilder.AppendLine("Referrer URL: " + ReferrerURL);
                strBuilder.AppendLine("Referrer’s Policy: " + ReferencePolicy.ToString());
                strBuilder.AppendLine("Original Request URL: " + OriginalRequestURL);

                if (UserAgent == 0)
                    strBuilder.AppendLine("The user-agent was not overridden.");
                else if (UserAgent == 1)
                    strBuilder.AppendLine("The user-agent was overridden.");
                else
                    strBuilder.AppendLine("User-agent: " + UserAgent.ToString());

                strBuilder.AppendLine("States:");
                strBuilder.Append(State.ToString());

                return strBuilder.ToString();
            }
        }

        public struct TabState
        {
            public struct StateV27 //Version 27 and 28
            {
                public string ValueA;   //UTF-16
                public string ValueB;   //UTF-16
                public string ValueC;   //UTF-16
                public byte[] ValueD;
                public List<string> ValueE;
                public long ValueF;
                public byte[] ValueG;
                public long ValueH;
                public long ValueI;
                public byte[] ValueJ;
                public byte[] ValueK;
                public string ValueL;   //ASCII, only in version 28
            }

            public int Version;
            public object States;

            public TabState(byte[] data)
            {
                Version = BitConverter.ToInt32(data, 4);
                long m0 = BitConverter.ToInt64(data, 12);
                long m1 = BitConverter.ToInt64(data, 20);
                long m2 = BitConverter.ToInt64(data, 28);
                long m3 = BitConverter.ToInt64(data, 36);

                if ((Version == 27 || Version == 28) &&
                    m0 == 0x18 &&
                    m1 == 0x10 &&
                    m2 == 0x10 &&
                    m3 == 0x08)
                {
                    List<StateV27> states = new List<StateV27>();
                    int offset = 44;
                    while (offset < data.Length)
                    {
                        StateV27 state = new StateV27();
                        int offsetA = BitConverter.ToInt32(data, offset + 8);
                        int offsetB = BitConverter.ToInt32(data, offset + 16);
                        int offsetC = BitConverter.ToInt32(data, offset + 24);
                        int offsetD = BitConverter.ToInt32(data, offset + 32);
                        int offsetE = BitConverter.ToInt32(data, offset + 40) + 40;
                        state.ValueF = BitConverter.ToInt64(data, offset + 48);
                        int offsetG = BitConverter.ToInt32(data, offset + 56);
                        state.ValueH = BitConverter.ToInt64(data, offset + 64);
                        state.ValueI = BitConverter.ToInt64(data, offset + 72);
                        int offsetJ = BitConverter.ToInt32(data, offset + 80) + 80;
                        int offsetK = BitConverter.ToInt32(data, offset + 88) + 88;
                        int offsetL = Version == 28 ? BitConverter.ToInt32(data, offset + 96) : 0;

                        int lengthE = BitConverter.ToInt32(data, offset + offsetE + 4);
                        int lengthJ = BitConverter.ToInt32(data, offset + offsetJ);
                        int lengthK = BitConverter.ToInt32(data, offset + offsetK);

                        if (offsetA != 0)
                        {
                            offsetA += 8;
                            int lengthA = BitConverter.ToInt32(data, offset + offsetA + 20) * 2;
                            state.ValueA = Encoding.Unicode.GetString(data, offset + offsetA + 24, lengthA);
                        }
                        else
                            state.ValueA = "";

                        if (offsetB != 0)
                        {
                            offsetB += 16;
                            int lengthB = BitConverter.ToInt32(data, offset + offsetB + 20) * 2;
                            state.ValueB = Encoding.Unicode.GetString(data, offset + offsetB + 24, lengthB);
                        }
                        else
                            state.ValueB = "";

                        if (offsetC != 0)
                        {
                            offsetC += 24;
                            int lengthC = BitConverter.ToInt32(data, offset + offsetC + 20) * 2;
                            state.ValueC = Encoding.Unicode.GetString(data, offset + offsetC + 24, lengthC);
                        }
                        else
                            state.ValueC = "";

                        if (offsetD != 0)
                        {
                            offsetD += 32;
                            int lengthD = BitConverter.ToInt32(data, offset + offsetD);
                            state.ValueD = new byte[lengthD];
                            Array.Copy(data, offset + offsetD, state.ValueD, 0, state.ValueD.Length);
                        }
                        else
                            state.ValueD = new byte[0];
                        
                        state.ValueE = new List<string>(lengthE);
                        for (int i = 0; i < lengthE; i++)
                        {
                            int offsetStr = BitConverter.ToInt32(data, offset + offsetE + (8 * (i + 1))) + (8 * (i + 1));
                            int lengthStr = BitConverter.ToInt32(data, offset + offsetE + offsetStr + 20) * 2;
                            state.ValueE.Add(Encoding.Unicode.GetString(data, offset + offsetE + offsetStr + 24, lengthStr));
                        }

                        if (offsetG != 0)
                        {
                            offsetG += 56;
                            int lengthG = offsetJ - offsetG;
                            state.ValueG = new byte[lengthG];
                            Array.Copy(data, offset + offsetG, state.ValueG, 0, state.ValueG.Length);
                        }
                        else
                            state.ValueG = new byte[0];

                        state.ValueJ = new byte[lengthJ];
                        Array.Copy(data, offset + offsetJ, state.ValueJ, 0, state.ValueJ.Length);
                        
                        state.ValueK = new byte[lengthK];
                        Array.Copy(data, offset + offsetK, state.ValueK, 0, state.ValueK.Length);

                        if (offsetL != 0)
                        {
                            offsetL += 96;
                            int lengthL = BitConverter.ToInt32(data, offset + offsetL + 4);
                            state.ValueL = Encoding.ASCII.GetString(data, offset + offsetL + 8, lengthL);
                            lengthL += 8;
                            offset += offsetL + (lengthL % 8 == 0 ? lengthL : lengthL / 8 * 8 + 8);
                        }
                        else
                        {
                            state.ValueL = "";
                            offset += offsetK + lengthK;
                        }

                        states.Add(state);
                    }
                    States = states;
                }
                else
                {
                    byte[] states = new byte[data.Length - 8];
                    Array.Copy(data, 8, states, 0, states.Length);
                    States = states;
                }
            }

            public override string ToString()
            {
                StringBuilder strBuilder = new StringBuilder();

                strBuilder.AppendLine("  Version: " + Version.ToString());

                if (States is List<StateV27>)
                {
                    List<StateV27> states = (List<StateV27>)States;
                    for (int i = 0; i < states.Count; i++)
                    {
                        strBuilder.AppendLine("  State " + i.ToString() + ":");
                        strBuilder.AppendLine("    Value A: " + states[i].ValueA);
                        strBuilder.AppendLine("    Value B: " + states[i].ValueB);
                        strBuilder.AppendLine("    Value C: " + states[i].ValueC);
                        strBuilder.Append("    Value D: ");
                        strBuilder.AppendLine(SNSS.ToString(states[i].ValueD));
                        strBuilder.AppendLine("    Value E:");
                        for (int j = 0; j < states[i].ValueE.Count; j++)                        
                            strBuilder.AppendLine("      Index " + j.ToString() + ": " + states[i].ValueE[j]);                        
                        strBuilder.AppendLine("    Value F: 0x" + states[i].ValueF.ToString("X16"));
                        strBuilder.Append("    Value G: ");
                        strBuilder.AppendLine(SNSS.ToString(states[i].ValueG));
                        strBuilder.AppendLine("    Value H: 0x" + states[i].ValueH.ToString("X16"));
                        strBuilder.AppendLine("    Value I: 0x" + states[i].ValueI.ToString("X16"));
                        strBuilder.Append("    Value J: ");
                        strBuilder.AppendLine(SNSS.ToString(states[i].ValueJ));
                        strBuilder.Append("    Value K: ");
                        strBuilder.AppendLine(SNSS.ToString(states[i].ValueK));
                        strBuilder.AppendLine("    Value L: " + states[i].ValueL);
                    }
                }
                else
                {
                    strBuilder.Append("  Value: ");
                    strBuilder.Append(SNSS.ToString((byte[])States));
                }

                return strBuilder.ToString();
            }
        }

        public string FileName;
        public int Version;
        public List<Command> Commands;

        public SNSS(string filename)
        {
            FileName = filename;
            Version = 0;
            Commands = new List<Command>();

            FileStream fs = File.Open(filename, FileMode.Open);
            byte[] magic = new byte[4];
            fs.Read(magic, 0, 4);

            if (magic[0] == 0x53 &&
                magic[1] == 0x4E &&
                magic[2] == 0x53 &&
                magic[3] == 0x53)
            {
                byte[] version = new byte[4];
                fs.Read(version, 0, 4);
                Version = BitConverter.ToInt32(version, 0);
                while (fs.Position < fs.Length)
                {
                    byte[] cmdSizeBytes = new byte[2];
                    fs.Read(cmdSizeBytes, 0, 2);
                    ushort commandSize = BitConverter.ToUInt16(cmdSizeBytes, 0);
                    byte[] command = new byte[commandSize];
                    fs.Read(command, 0, commandSize);
                    Commands.Add(new Command(command));
                }
            }

            fs.Close();
        }

        public override string ToString()
        {
            StringBuilder strBuilder = new StringBuilder();

            strBuilder.AppendLine("File: " + FileName);
            if (Version != 0)
            {
                strBuilder.AppendLine("Version: " + Version.ToString());
                strBuilder.AppendLine("Session commands: " + Commands.Count.ToString());
            }
            else
                strBuilder.AppendLine("It is not an file with SNSS format.");

            return strBuilder.ToString();
        }

        private static string ToString(byte[] value)
        {
            StringBuilder hex = new StringBuilder(value.Length * 2);
            foreach (byte b in value)
                hex.AppendFormat("{0:X2} ", b);
            return hex.ToString();
        }
    }
}
