namespace ResistileServer
{
    class Coordinates
    {
        public int[] coordinates = new int[2];

        public Coordinates(int x, int y)
        {
            coordinates[0] = x;
            coordinates[1] = y;
        }

        public int x()
        {
            return coordinates[0];
        }

        public int y()
        {
            return coordinates[1];
        }

        public Coordinates getDirection(string direction)
        {
            switch (direction)
            {
                case Directions.up:
                    return up();
                case Directions.down:
                    return down();
                case Directions.left:
                    return left();
                default:
                    return right();
            }
        }
        public Coordinates up()
        {
            return new Coordinates(coordinates[0], coordinates[1] - 1);
        }

        public Coordinates left()
        {
            return new Coordinates(coordinates[0] - 1, coordinates[1]);
        }

        public Coordinates down()
        {
            return new Coordinates(coordinates[0], coordinates[1] + 1);
        }

        public Coordinates right()
        {
            return new Coordinates(coordinates[0] + 1, coordinates[1]);
        }
    }
}