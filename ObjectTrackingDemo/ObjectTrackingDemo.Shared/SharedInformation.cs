using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectTrackingDemo
{
    static  class SharedInformation
    {
        public static String roomCurrent = "";
        public static String myName = "";
        public static String myNumber = "";
        public static String myColor = "";
        public static String myScore = "";

        public static Rooms savedRoom = new Rooms();

        public static List<Player> ListOfPlayers = new List<Player>();

    }
}
