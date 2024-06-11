using HarmonyLib;
using LightWeightJsonParser;
#if TAIKO_IL2CPP
using Il2CppInterop.Runtime.InteropTypes.Arrays;
#endif
using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace AddUraToBaseGameSong.Patches
{
    internal class AddUraToBaseGameSongPatch
    {
        static Dictionary<string, ChartData> UraCharts = new Dictionary<string, ChartData>();

        [HarmonyPatch(typeof(DataManager), nameof(DataManager.Awake))]
        [HarmonyPostfix]
        [HarmonyWrapSafe]
        private static void DataManager_PostFix(DataManager __instance)
        {
            if (__instance.MusicData != null)
            {
                MusicDataInterface_Postfix_New(__instance.MusicData, Application.streamingAssetsPath + "/ReadAssets/musicinfo.bin");
            }
        }

        public static void MusicDataInterface_Postfix_New(MusicDataInterface __instance, string path)
        {
            var uraPath = Plugin.Instance.ConfigUraChartPath.Value;
            DirectoryInfo dirInfo = new DirectoryInfo(uraPath);
            var subDirs = dirInfo.GetDirectories();
            for (int i = 0; i < subDirs.Length; i++)
            {
                var dataFiles = subDirs[i].GetFiles("data.json");
                if (dataFiles.Length > 0)
                {
                    var file = dataFiles[0];
                    var node = LWJson.Parse(File.ReadAllText(file.FullName));
                    ChartData data = new ChartData();
                    data.SongId = node["SongId"].AsString();
                    data.IsBranch = node["Branch"].AsBoolean();
                    data.Stars = node["Stars"].AsInteger();
                    data.Points = node["Points"].AsInteger();
                    data.PointsDuet = node["PointsDuet"].AsInteger();
                    data.Score = node["Score"].AsInteger();

                    data.HasDuet = false;
                    if (File.Exists(Path.Combine(subDirs[i].FullName, data.SongId, data.SongId + "_x_1.bin")) &&
                        File.Exists(Path.Combine(subDirs[i].FullName, data.SongId, data.SongId + "_x_2.bin")))
                    {
                        data.HasDuet = true;
                    }


                    if (!UraCharts.ContainsKey(data.SongId) && File.Exists(Path.Combine(subDirs[i].FullName, data.SongId + "_x.bin")))
                    {
                        UraCharts.Add(data.SongId, data);
                    }
                }
            }

            for (int i = 0; i < __instance.musicInfoAccessers.Count; i++)
            {
                if (UraCharts.ContainsKey(__instance.musicInfoAccessers[i].Id))
                {
                    var data = UraCharts[__instance.musicInfoAccessers[i].Id];
                    __instance.musicInfoAccessers[i].Stars[4] = data.Stars;
                    __instance.musicInfoAccessers[i].ShinutiScores[4] = data.Points;
                    __instance.musicInfoAccessers[i].ShinutiScoreDuets[4] = data.PointsDuet;
                    __instance.musicInfoAccessers[i].Scores[4] = data.Score;
                    Plugin.LogInfo("Ura Chart Added: " + __instance.musicInfoAccessers[i].Id);
                }
            }
        }

        
        [HarmonyPatch(typeof(Cryptgraphy), nameof(Cryptgraphy.ReadAllAesAndGZipBytes))]
        [HarmonyPrefix]
        private static bool ReadAllAesAndGZipBytes_Prefix(Cryptgraphy __instance, string path, Cryptgraphy.AesKeyType type,
#if TAIKO_IL2CPP
        ref Il2CppStructArray<byte> __result
#elif TAIKO_MONO
    ref byte[] __result
#endif
)
        {
            FileInfo file = new FileInfo(path);
            if (UraCharts.ContainsKey(file.Directory.Name) && (path.EndsWith("_x.bin") || path.EndsWith("_x_1.bin") || path.EndsWith("_x_2.bin")))
            {
                string fileEnding = "_x.bin";
                if (path.EndsWith("_x_1.bin"))
                {
                    fileEnding = "_x_1.bin";
                }
                else if (path.EndsWith("_x_2.bin"))
                {
                    fileEnding = "_x_2.bin";
                }
                var data = UraCharts[file.Directory.Name];
                bool gzipped = true;
                var uraPath = Plugin.Instance.ConfigUraChartPath.Value;

                string filePath = Path.Combine(uraPath, data.SongId, data.SongId + fileEnding);
                var tmpBytes = File.ReadAllBytes(filePath);
                List<byte> gzippedFileHeader = new List<byte>() { 0x1F, 0x8B, 0x08 };
                for (int i = 0; i < gzippedFileHeader.Count; i++)
                {
                    if (tmpBytes[i] != gzippedFileHeader[i])
                    {
                        gzipped = false;
                    }
                }
                if (!gzipped)
                {
                    __result = tmpBytes;
                    return false;
                }
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    MemoryStream memoryStream = new MemoryStream();
                    using (GZipStream gzipStream = new GZipStream(fs, CompressionMode.Decompress))
                    {
                        gzipStream.CopyTo(memoryStream);
                    }
                    __result = memoryStream.ToArray();
                }
                return false;
            }

            return true;
        }
    }
}
