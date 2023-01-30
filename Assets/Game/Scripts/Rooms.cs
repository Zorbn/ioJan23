namespace Game.Scripts
{
    public static class Rooms
    {
        public const char Air = ' ';
        public const char Solid = '#';
        public const char Spawn = 'S';
        public const char Exit = 'E';
        public const char Enemy = 'X';
        public const char Spike = '^';
        public const int RoomWidth = 10;
        public const int RoomHeight = 8;

        public static readonly string[] AllOpenSpawn = {
            "####  ####" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#   S    #" +
            "#  ####  #" +
            "          " +
            "####  ####"
        };
        
        public static readonly string[] AllOpenExit = {
            "####  ####" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "      E   " +
            "####  ####"
        };

        public static readonly string[] LeftRightOpen = {
            "##########" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "    X     " +
            "##########"
        };
        
        public static readonly string[] AllOpen = {
            "####  ####" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "  ^       " +
            "####  ####"
        };

        public static readonly string[] Optional = {
            "##########" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "#        #" +
            "##########"
        };
    }
}