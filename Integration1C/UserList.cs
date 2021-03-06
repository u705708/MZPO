﻿using System.Collections.Generic;
using System.Linq;

namespace Integration1C
{
    public static class UserList
    {
        private static readonly List<(int, string)> _users = new()
        {
            (6729241, "Айбасов Серик"),
            (2375131, "Алферова Лилия"),
            (6410290, "Бармина Вероника"),
            (2375143, "Белоусова Екатерина"),
            (2375122, "Васина Елена"),
            (2976226, "Гладкова Вера"),
            (2375107, "Гребенникова Кристина"),
            (6630727, "Зубатых Елена"),
            (2375116, "Киреева Светлана"),
            (3835801, "Кубышина Наталья"),
            (6102562, "Лукьянова Валерия"),
            (6158035, "Матюк Анастасия Витальевна"),
            (6346882, "Мусихина Юлия"),
            (2375152, "Оганисян Карен"),
            (2884132, "Сорокина Ирина Витальевна"),
            (3813670, "Федорова Александра"),
            (6028753, "Федосова Алена"),
            (6697522, "Филатова Наталья"),
            (6200629, "Харшиладзе Леван"),
            (3770773, "Шталева Лидия"),
            (7074307, "Димитренко Татьяна Владимировна"),
            (7074316, "Суховерхова Евгения Юрьевна"),
            (7074319, "Фиданца Людмила Николаевна"),
            (7065112, "Тетеревятников Станислав"),
        };

        public static string Get1CUser(int? user_id_amo)
        {
            if (user_id_amo is not null &&
                _users.Any(x => x.Item1 == user_id_amo))
                return _users.First(x => x.Item1 == user_id_amo).Item2;
            return null;
        }

        public static int? GetAmoUser(string user_id_1C)
        {
            if (_users.Any(x => x.Item2 == user_id_1C))
                return _users.First(x => x.Item2 == user_id_1C).Item1;
            return null;
        }
    }
}