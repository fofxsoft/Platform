using System;

namespace Console
{
    public class Point
    {
        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public Point(string x, string y)
        {
            X = 0;
            Y = 0;

            if (double.TryParse(x, out double xv))
            {
                X = xv;
            }
            else
            {
                throw new Exception("Invalid X coordinate.");
            }

            if (double.TryParse(y, out double yv))
            {
                Y = yv;
            }
            else
            {
                throw new Exception("Invalid Y coordinate.");
            }
        }

        public Point(string coord)
        {
            X = 0;
            Y = 0;

            string[] coords = coord.Split(',');

            if (coords.Length == 2)
            {
                if (double.TryParse(coords[0], out double x))
                {
                    X = x;
                }
                else
                {
                    throw new Exception("Invalid X coordinate.");
                }

                if (double.TryParse(coords[1], out double y))
                {
                    Y = y;
                }
                else
                {
                    throw new Exception("Invalid Y coordinate.");
                }
            }
            else
            {
                throw new Exception("Invalid coordinates \"X, Y\".");
            }
        }

        public double X
        {
            get;
            set;
        }

        public double Y
        {
            get;
            set;
        }
    }
}
