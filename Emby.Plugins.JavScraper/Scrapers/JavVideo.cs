﻿using MediaBrowser.Model.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Emby.Plugins.JavScraper.Scrapers
{
    /// <summary>
    /// 视频
    /// </summary>
    public class JavVideo : JavVideoIndex
    {
        /// <summary>
        /// 原始标题
        /// </summary>
        private string _originalTitle;

        /// <summary>
        /// 原始标题
        /// </summary>
        public string OriginalTitle { get => string.IsNullOrWhiteSpace(_originalTitle) ? (_originalTitle = Title) : _originalTitle; set => _originalTitle = value; }

        /// <summary>
        /// 内容简介
        /// </summary>
        public string Plot { get; set; }

        /// <summary>
        /// 导演
        /// </summary>
        public string Director { get; set; }

        /// <summary>
        /// 影片时长
        /// </summary>
        public string Runtime { get; set; }

        /// <summary>
        /// 制作组
        /// </summary>
        public string Studio { get; set; }

        /// <summary>
        /// 厂商
        /// </summary>
        public string Maker { get; set; }

        /// <summary>
        /// 合集
        /// </summary>
        public string Set { get; set; }

        /// <summary>
        /// 类别
        /// </summary>
        public List<string> Genres { get; set; }

        /// <summary>
        /// 演员
        /// </summary>
        public List<string> Actors { get; set; }

        /// <summary>
        /// 样品图片
        /// </summary>
        public List<string> Samples { get; set; }

        /// <summary>
        /// %genre:中文字幕?中文:%
        /// </summary>
        private static Regex regex_genre = new Regex("%genre:(?<a>[^?]+)?(?<b>[^:]*):(?<c>[^%]*)%", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string GetFormatName(string name, string empty)
        {
            if (empty == null)
                empty = string.Empty;

            var m = this;
            void Replace(string key, string value)
            {
                var _index = name.IndexOf(key, StringComparison.OrdinalIgnoreCase);
                if (_index < 0)
                    return;

                if (string.IsNullOrEmpty(value))
                    value = empty;

                do
                {
                    name = name.Remove(_index, key.Length);
                    name = name.Insert(_index, value);
                    _index = name.IndexOf(key, _index + value.Length, StringComparison.OrdinalIgnoreCase);
                } while (_index >= 0);
            }

            Replace("%num%", m.Num);
            Replace("%title%", m.Title);
            Replace("%title_original%", m.OriginalTitle);
            Replace("%actor%", m.Actors?.Any() == true ? string.Join(", ", m.Actors) : null);
            Replace("%actor_first%", m.Actors?.FirstOrDefault());
            Replace("%set%", m.Set);
            Replace("%director%", m.Director);
            Replace("%date%", m.Date);
            Replace("%year%", m.GetYear()?.ToString());
            Replace("%month%", m.GetMonth()?.ToString("00"));
            Replace("%studio%", m.Studio);
            Replace("%maker%", m.Maker);

            do
            {
                //%genre:中文字幕?中文:%
                var match = regex_genre.Match(name);
                if (match.Success == false)
                    break;
                var a = match.Groups["a"].Value;
                var genre_key = m.Genres?.Contains(a, StringComparer.OrdinalIgnoreCase) == true ? "b" : "c";
                var genre_value = match.Groups[genre_key].Value;
                name = name.Replace(match.Value, genre_value);
            } while (true);

            return name;
        }

        /// <summary>
        /// 保存到缓存
        /// </summary>
        /// <param name="cachePath"></param>
        /// <param name="_jsonSerializer"></param>
        /// <returns></returns>
        public bool SaveToCache(string cachePath, IJsonSerializer _jsonSerializer)
        {
            try
            {
                cachePath = Path.Combine(cachePath, Plugin.NAME, Provider, $"{Num}.json");
                Directory.CreateDirectory(Path.GetDirectoryName(cachePath));
                _jsonSerializer.SerializeToFile(this, cachePath);
                return true;
            }
            catch
            {
            }
            return false;
        }

        /// <summary>
        /// 从缓存中读取
        /// </summary>
        /// <param name="cachePath"></param>
        /// <param name="_jsonSerializer"></param>
        /// <returns></returns>
        public JavVideo LoadFromCache(string cachePath, IJsonSerializer _jsonSerializer)
        {
            try
            {
                cachePath = Path.Combine(cachePath, Plugin.NAME, Provider, $"{Num}.json");
                if (File.Exists(cachePath))
                    return _jsonSerializer.DeserializeFromFile<JavVideo>(cachePath);
            }
            catch
            {
            }
            return null;
        }
    }
}