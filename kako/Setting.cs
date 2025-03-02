﻿using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace kako
{
    public static class Setting
    {
        private static Data _data = new();

        #region データクラス
        /// <summary>
        /// 設定データクラス
        /// </summary>
        public class Data
        {
            public Point Location { get; set; }
            public Size Size { get; set; } = new Size(400, 400);
            public int NameColumnWidth { get; set; } = 70;

            public bool TopMost { get; set; } = false;
            public double Opacity { get; set; } = 1.00;
            public bool ShowOnlyFollowees { get; set; } = true;
            public bool MinimizeToTray { get; set; } = false;
            public bool AddClient { get; set; } = true;
            public string Director { get; set; } = string.Empty;
            public List<string> ReplyCommands { get; set; } = [];
            public string CallCommand { get; set; } = string.Empty;
            public bool OpenMode { get; set; } = false;
            public bool MentionEveryHour { get; set; } = false;
            public int MentionMinutes { get; set; } = 0;

            public string GridColor { get; set; } = "#FF1493";
            public string ReactionColor { get; set; } = "#FFFFE0";
            public string ReplyColor { get; set; } = "#E6E6FA";
            public string CWColor { get; set; } = "#C0C0C0";
        }
        #endregion

        #region プロパティ
        public static Point Location
        {
            get => _data.Location;
            set => _data.Location = value;
        }
        public static Size Size
        {
            get => _data.Size;
            set => _data.Size = value;
        }
        public static int NameColumnWidth
        {
            get => _data.NameColumnWidth;
            set => _data.NameColumnWidth = value;
        }

        public static bool TopMost
        {
            get => _data.TopMost;
            set => _data.TopMost = value;
        }
        public static double Opacity
        {
            get => _data.Opacity;
            set => _data.Opacity = value;
        }
        public static bool ShowOnlyFollowees
        {
            get => _data.ShowOnlyFollowees;
            set => _data.ShowOnlyFollowees = value;
        }
        public static bool MinimizeToTray
        {
            get => _data.MinimizeToTray;
            set => _data.MinimizeToTray = value;
        }
        public static bool AddClient
        {
            get => _data.AddClient;
            set => _data.AddClient = value;
        }
        public static string Director
        {
            get => _data.Director;
            set => _data.Director = value;
        }
        public static List<string> ReplyCommands
        {
            get => _data.ReplyCommands;
            set => _data.ReplyCommands = value;
        }
        public static string CallCommand
        {
            get => _data.CallCommand;
            set => _data.CallCommand = value;
        }
        public static bool OpenMode
        {
            get => _data.OpenMode;
            set => _data.OpenMode = value;
        }
        public static bool MentionEveryHour
        {
            get => _data.MentionEveryHour;
            set => _data.MentionEveryHour = value;
        }
        public static int MentionMinutes
        {
            get => _data.MentionMinutes;
            set => _data.MentionMinutes = value;
        }

        public static string GridColor
        {
            get => _data.GridColor;
            set => _data.GridColor = value;
        }
        public static string ReactionColor
        {
            get => _data.ReactionColor;
            set => _data.ReactionColor = value;
        }
        public static string ReplyColor
        {
            get => _data.ReplyColor;
            set => _data.ReplyColor = value;
        }
        public static string CWColor
        {
            get => _data.CWColor;
            set => _data.CWColor = value;
        }
        #endregion

        #region 設定ファイル操作
        /// <summary>
        /// 設定ファイル読み込み
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Load(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Data));
                var xmlSettings = new XmlReaderSettings();
                using var streamReader = new StreamReader(path, Encoding.UTF8);
                using var xmlReader = XmlReader.Create(streamReader, xmlSettings);
                _data = serializer.Deserialize(xmlReader) as Data ?? _data;
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// 設定ファイル書き込み
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool Save(string path)
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Data));
                using var streamWriter = new StreamWriter(path, false, Encoding.UTF8);
                serializer.Serialize(streamWriter, _data);
                streamWriter.Flush();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
        #endregion
    }
}
