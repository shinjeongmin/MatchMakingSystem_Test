using UnityEngine;

public class BattleLog
{
    public BattleLog() {}

    public string name;
    public int year;
    public int month;
    public int date;

    public Nested nested;

    public void WriteBattleLog(string n, int y, int m, int d)
    {
        name = n;
        year = y;
        month = m;
        date = d;
    }

    public class Nested
    {
        public int x = 0;
        public string name = "sin";

        public Nested2 nested2 = new Nested2();

        public class Nested2
        {
            public string name2 = "Oh My God Them";

            public Nested3 nested3 = new Nested3();

            public class Nested3
            {
                public string name3 = "God you Jesus! Holy Moly!";
            }
        }
    }
}