using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddUraToBaseGameSong.Patches
{
    internal class ChartData
    {
        public string SongId { get; set; }
        public bool IsBranch { get; set; }
        public int Stars { get; set; }
        public int Points { get; set; }
        public int PointsDuet { get; set; }
        public int Score { get; set; }
        public bool HasDuet { get; set; }
    }
}
