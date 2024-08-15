﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CommunityToolkit.Mvvm.ComponentModel;

namespace OfficeTools.Models;

public class WordPlainItem : ObservableObject
{
    private string _content;
    private string _contentType;
    private int _id;

    [Range(0, 10000)]
    public int Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }

    [Length(0, 10)]
    public string ContentType
    {
        get => _contentType;
        set => SetProperty(ref _contentType, value);
    }

    [Required]
    public string Content
    {
        get => _content;
        set => SetProperty(ref _content, value);
    }
}

public class SongViewModel : ObservableObject
{
    private string? _album;
    private string? _artist;
    private int _countOfComment;
    private bool? _isSelected;
    private string? _title;

    public string? Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    public string? Artist
    {
        get => _artist;
        set => SetProperty(ref _artist, value);
    }

    public string? Album
    {
        get => _album;
        set => SetProperty(ref _album, value);
    }

    public int CountOfComment
    {
        get => _countOfComment;
        set => SetProperty(ref _countOfComment, value);
    }

    public bool? IsSelected
    {
        get => _isSelected;
        set => SetProperty(ref _isSelected, value);
    }

    public TimeSpan? Duration { get; set; }
    public int NetEaseId { get; set; }
}

public class Song
{
    public Song(string title, string artist, int m, int s, string album, int countOfComment, int netEaseId)
    {
        Title = title;
        Artist = artist;
        Duration = new TimeSpan(0, m, s);
        Album = album;
        CountOfComment = countOfComment;
        Url = $"https://music.163.com/song?id={netEaseId}";
    }

    public string? Title { get; set; }
    public string? Artist { get; set; }
    public TimeSpan? Duration { get; set; }
    public string? Album { get; set; }
    public int CountOfComment { get; set; }
    public string Url { get; set; }

    public static List<Song> Songs { get; set; } = new()
    {
        new Song("好肚有肚(feat.李玲玉)", "熊猫堂ProducePandas", 2, 50, "A.S.I.A", 730, 1487039339),
        new Song("荒诞秀", "熊猫堂ProducePandas", 3, 15, "A.S.I.A", 639, 1487037601),
        new Song("长大", "熊猫堂ProducePandas", 4, 6, "A.S.I.A", 1114, 1487037690),
        new Song("招财猫(feat.纪粹希(G-Tracy))", "熊猫堂ProducePandas", 3, 37, "A.S.I.A", 361, 1487039632),
        new Song("千转", "熊猫堂ProducePandas", 4, 0, "A.S.I.A", 1115, 1477312398),
        new Song("辣辣辣", "熊猫堂ProducePandas", 3, 24, "A.S.I.A", 1873, 1465043716),
        new Song("碎碎念", "熊猫堂ProducePandas", 3, 25, "A.S.I.A", 676, 1474142064),
        new Song("盘他", "熊猫堂ProducePandas", 2, 16, "A.S.I.A", 365, 1481652786),
        new Song("Na Na Na", "熊猫堂ProducePandas", 3, 26, "A.S.I.A", 312, 1469022662),
        new Song("Indigo", "熊猫堂ProducePandas", 3, 15, "A.S.I.A", 137, 1487039517),
        new Song("饕餮人间", "熊猫堂ProducePandas", 3, 20, "饕餮人间", 1295, 1499584605),
        new Song("七步咙咚呛", "熊猫堂ProducePandas", 3, 10, "七步咙咚呛", 175, 1809095152),
        new Song("大惊小怪", "熊猫堂ProducePandas", 3, 32, "大惊小怪", 10420, 1847477425),
        new Song("工具人", "熊猫堂ProducePandas", 2, 46, "大惊小怪", 1135, 1847476499),
        new Song("以梦为马", "熊猫堂ProducePandas", 4, 19, "大惊小怪", 18361, 1836034373),
        new Song("以梦为马(Piano Version)", "熊猫堂ProducePandas", 3, 4, "大惊小怪", 570, 1847477423),
        new Song("The ONE", "熊猫堂ProducePandas", 2, 58, "The ONE", 1508, 1864329424),
        new Song("The ONE(日文版)", "熊猫堂ProducePandas", 2, 57, "The ONE", 385, 1864329429),
        new Song("以梦为马 (壮志骄阳版)", "熊猫堂ProducePandas", 4, 19, "以梦为马 (壮志骄阳版)", 161, 1865138896),
        new Song("New Horse", "熊猫堂ProducePandas", 2, 30, "emo了", 643, 1887021307),
        new Song("不例外", "熊猫堂ProducePandas", 3, 31, "emo了", 1818, 1887022665),
        new Song("满意", "熊猫堂ProducePandas", 4, 32, "emo了", 1081, 1882433472),
        new Song("就算与全世界为敌也要跟你在一起", "熊猫堂ProducePandas", 3, 32, "emo了", 2119, 1881759960),
        new Song("The ONE", "熊猫堂ProducePandas", 2, 58, "emo了", 67, 1887022648),
        new Song("口香糖", "熊猫堂ProducePandas", 3, 10, "emo了", 2181, 1885502254),
        new Song("Suuuuuuper Mario", "熊猫堂ProducePandas", 3, 32, "emo了", 1010, 1887021318),
        new Song("饕餮人间", "熊猫堂ProducePandas", 3, 22, "emo了", 109, 1887021320),
        new Song("以梦为马 (壮志骄阳版)", "熊猫堂ProducePandas", 4, 21, "emo了", 34, 1887022666),
        new Song("The ONE(日文版)", "熊猫堂ProducePandas", 2, 57, "emo了", 27, 1887022646),
        new Song("满意(DJheap九天版)", "熊猫堂ProducePandas", 4, 31, "emo了", 31, 1901605941),
        new Song("一眼万年", "熊猫堂ProducePandas", 3, 54, "一眼万年", 20, 1922599361),
        new Song("冲刺", "熊猫堂ProducePandas", 3, 49, "冲刺吧", 1006, 1932878194),
        new Song("滴答滴", "熊猫堂ProducePandas", 2, 30, "爱的赏味期限", 86, 1957515790),
        new Song("热带季风", "熊猫堂ProducePandas", 2, 45, "爱的赏味期限", 212, 1957514964),
        new Song("渣", "熊猫堂ProducePandas", 3, 28, "爱的赏味期限", 22, 1957514965),
        new Song("独特", "熊猫堂ProducePandas", 3, 33, "爱的赏味期限", 62, 1957514966),
        new Song("雨后", "熊猫堂ProducePandas", 4, 15, "爱的赏味期限", 23, 1957514967),
        new Song("然后然后", "熊猫堂ProducePandas", 3, 50, "爱的赏味期限", 108, 1957514968),
        new Song("丢", "熊猫堂ProducePandas", 3, 26, "爱的赏味期限", 30, 1957515792),
        new Song("热带疾风(FACEVOID桃心连哥 Remix)", "熊猫堂ProducePandas", 3, 23, "爱的赏味期限", 55, 1957515793),
        new Song("COSMIC ANTHEM -Japanese Ver.-", "熊猫堂ProducePandas", 3, 11, "COSMIC ANTHEM / 手紙", 0, 1977171493),
        new Song("手紙 (「長大-You Raise Me Up-」-Japanese Ver.-)",
            "熊猫堂ProducePandas",
            4,
            11,
            "COSMIC ANTHEM / 手紙",
            0,
            1977171494
        ),
        new Song("COSMIC ANTHEM -Chinese Ver.-", "熊猫堂ProducePandas", 3, 31, "COSMIC ANTHEM / 手紙", 0, 1977172202),
        new Song("世界晚安", "熊猫堂ProducePandas", 2, 59, "世界晚安", 652, 1985063377),
        new Song("世界晚安(泰文版)", "熊猫堂ProducePandas", 2, 59, "世界晚安", 134, 1987842504),
        new Song("世界晚安(钢琴版)", "熊猫堂ProducePandas", 3, 2, "世界晚安", 76, 1990475933),
        new Song("世界晚安(泰文钢琴版)", "熊猫堂ProducePandas", 3, 2, "世界晚安", 29, 1990475934),
        new Song("世界晚安(DJ沈念版)", "熊猫堂ProducePandas", 3, 9, "世界晚安", 34, 2014263184),
        new Song("世界晚安(钢琴配乐)", "熊猫堂ProducePandas", 2, 59, "世界晚安", 11, 2014263185),
        new Song("明年也要好好长大", "熊猫堂ProducePandas", 3, 12, "明年也要好好长大", 0, 2010515162),
        new Song("320万年前（DJ沈念版）", "熊猫堂ProducePandas", 3, 21, "320万年前", 8, 2055888636),
        new Song("320万年前", "熊猫堂ProducePandas", 3, 7, "W.O.R.L.D.", 329, 2049770469),
        new Song("隐德来希", "熊猫堂ProducePandas", 3, 3, "W.O.R.L.D.", 594, 2061317924),
        new Song("孔明", "熊猫堂ProducePandas", 3, 59, "W.O.R.L.D.", 91, 2063175274),
        new Song("锦鲤卟噜噜", "熊猫堂ProducePandas", 3, 5, "W.O.R.L.D.", 67, 2059208262),
        new Song("指鹿为马", "熊猫堂ProducePandas", 3, 12, "W.O.R.L.D.", 74, 2063175272),
        new Song("热带季风Remix", "熊猫堂ProducePandas", 3, 22, "W.O.R.L.D.", 23, 2063173319),
        new Song("加州梦境", "熊猫堂ProducePandas", 2, 56, "W.O.R.L.D.", 1662, 2063173324),
        new Song("渐近自由", "熊猫堂ProducePandas", 4, 19, "W.O.R.L.D.", 124, 2063173321),
        new Song("世界所有的烂漫", "熊猫堂ProducePandas", 3, 30, "W.O.R.L.D.", 335, 2053388775)
    };
}