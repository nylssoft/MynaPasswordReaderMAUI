/*
    Myna Password Reader MAUI
    Copyright (C) 2022 Niels Stockfleth

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
namespace PasswordReader.Pages
{
    public class DiaryPageDrawable : IDrawable
    {
        public DiaryPageDrawable()
        {
            var now = DateTime.Now;
            Month = now.Month;
            Year = now.Year;
        }

        private int _width;

        public int Month { get; set; }

        public int Year { get; set; }

        public List<int> Days { get; set; }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            var days = new[] { "Mon", "Die", "Mit", "Don", "Fre", "Sam", "Son" };
            var date = new DateTime(Year, Month, 1);
            int firstDay = (int)date.DayOfWeek - 1;
            if (firstDay < 0)
            {
                firstDay = 6;
            }
            int daysInMonth = DateTime.DaysInMonth(Year, Month);
            int w = Convert.ToInt32(Math.Min(dirtyRect.Width, dirtyRect.Height)) / 8;
            _width = w;
            canvas.FontSize = 14;
            canvas.FontColor = Colors.White;
            for (int x=0; x<7; x++)
            {
                canvas.DrawString(days[x], x * w + w / 2, w / 2 + w / 4, HorizontalAlignment.Center);
            }
            canvas.FontSize = 18;
            int day = 1;
            var today = DateTime.Now;
            for (int y = 0; y < 6; y++)
            {
                canvas.FillColor = y % 2 == 0 ? Color.FromRgb(100,149,237) : Color.FromRgb(30, 144, 255);
                for (int x = 0; x < 7; x++)
                {
                    canvas.FillRectangle(x * w, y * w + w, w, w);
                    if (y == 0 && x < firstDay  || day > daysInMonth)
                    {
                        continue;
                    }
                    var isToday = day == today.Day && Year == today.Year && Month == today.Month;
                    var msg = $"{day}" + (isToday ? "*" : "");
                    if (Days != null && Days.Contains(day))
                    {
                        canvas.FontColor = Colors.Yellow;
                    }
                    else
                    {
                        canvas.FontColor = Colors.White;
                    }
                    canvas.DrawString(msg, x * w + w / 2, y * w + w / 2 + w / 4 + w, HorizontalAlignment.Center);
                    day++;
                }
            }
            canvas.StrokeColor = Colors.Blue;
            canvas.DrawRectangle(0, w, 7 * w, 6 * w);
        }

        public int? GetDay(float tx, float ty)
        {
            if (_width > 0)
            {
                var rx = Convert.ToInt32((tx - _width / 2) / _width);
                var ry = Convert.ToInt32((ty - 1.5 * _width) / _width);
                if (rx >= 0 && rx <= 6 && ry >= 0 && ry <= 5)
                {
                    int day = 1;
                    var date = new DateTime(Year, Month, 1);
                    int firstDay = (int)date.DayOfWeek - 1;
                    if (firstDay < 0)
                    {
                        firstDay = 6;
                    }
                    int daysInMonth = DateTime.DaysInMonth(Year, Month);
                    for (int y = 0; y < 6; y++)
                    {
                        for (int x = 0; x < 7; x++)
                        {
                            if (y == 0 && x < firstDay || day > daysInMonth)
                            {
                                continue;
                            }
                            if (y == ry && x == rx)
                            {
                                return day;
                            }
                            day++;
                        }
                    }
                }
            }
            return null;
        }
    }
}
