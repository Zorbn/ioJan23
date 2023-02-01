namespace Game.Scripts
{
    public static class Rooms
    {
        public const char Air = ' ';
        public const char Solid = '#';
        public const char Spawn = '[';
        public const char Exit = ']';
        public const char Enemy = 'E';
        public const char Spike = '^';
        public const int RoomWidth = 10;
        public const int RoomHeight = 8;

        public static readonly string[] AllOpenSpawn = {
            "####  ####" +
            "#        #" +
            "#^      ^#" +
            "##      ##" +
            "#   [    #" +
            "#  ####  #" +
            "          " +
            "####  ####"
        };
        
        public static readonly string[] AllOpenExit = {
            "####  ####" +
            "##      ##" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "      ]   " +
            "####  ####"
        };

        public static readonly string[] LeftRightOpen = {
            "##########" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "    E     " +
            "##########"
        };
        
        public static readonly string[] AllOpen = {
            "####  ####" +
            "##      ##" +
            "#   E    #" +
            "# #####  #" +
            "#        #" +
            "##      ##" +
            "          " +
            "####  ####",
            "####  ####" +
            "#        #" +
            "# #    # #" +
            "# #^  ^# #" +
            "# ##  ## #" +
            "#        #" +
            "  #    #  " +
            "####  ####",
            "####  ####" +
            "#        #" +
            "#      E##" +
            "#     ####" +
            "##       #" +
            "###      #" +
            "      #   " +
            "####  ####",
            "####  ####" +
            "#   #    #" +
            "#     ####" +
            "##   #####" +
            "###      #" +
            "#      ###" +
            "   #      " +
            "####  ####"
        };

        public static readonly string[] Optional = {
            "####  ####" +
            "#        #" +
            "###      #" +
            "##     ###" +
            "#     ####" +
            "###      #" +
            "#####^^^^#" +
            "##########",
            "##########" +
            "###    ###" +
            "##      ##" +
            "##      ##" +
            "#        #" +
            "#        #" +
            "#  #^^# E " +
            "##########",
            "##########" +
            "###    ###" +
            "##      ##" +
            "##      ##" +
            "#        #" +
            "#        #" +
            " E #^^#  #" +
            "##########",
            "##########" +
            "#E       #" +
            "###      #" +
            "####     #" +
            "###      #" +
            "####    ##" +
            "#      ###" +
            "####  ####"
        };
    }
}