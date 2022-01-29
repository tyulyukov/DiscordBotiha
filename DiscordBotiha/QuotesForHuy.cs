using System;
using System.Collections.Generic;

namespace DiscordBotiha
{
    public static class QuotesForHuy
    {
        public static String RandomLessThanZero => QuotesForHuy.LessThenZero[random.Next(0, QuotesForHuy.LessThenZero.Count)];
        public static String RandomIsZero => QuotesForHuy.IsZero[random.Next(0, QuotesForHuy.IsZero.Count)];
        public static String RandomLessThenFive => QuotesForHuy.LessThenFive[random.Next(0, QuotesForHuy.LessThenFive.Count)];
        public static String RandomIsEight => QuotesForHuy.IsEight[random.Next(0, QuotesForHuy.IsEight.Count)];
        public static String RandomLessThenEight => QuotesForHuy.LessThenEight[random.Next(0, QuotesForHuy.LessThenEight.Count)];
        public static String RandomLessThenEleven => QuotesForHuy.LessThenEleven[random.Next(0, QuotesForHuy.LessThenEleven.Count)];
        public static String RandomLessThenFifteen => QuotesForHuy.LessThenFifteen[random.Next(0, QuotesForHuy.LessThenFifteen.Count)];
        public static String RandomLessThenTwenty => QuotesForHuy.LessThenTwenty[random.Next(0, QuotesForHuy.LessThenTwenty.Count)];
        public static String RandomLessThenTwentyFive => QuotesForHuy.LessThenTwentyFive[random.Next(0, QuotesForHuy.LessThenTwentyFive.Count)];
        public static String RandomIsTwentyFive => QuotesForHuy.IsTwentyFive[random.Next(0, QuotesForHuy.IsTwentyFive.Count)];

        private static Random random = new Random();

        private readonly static List<String> LessThenZero = new List<String>()
        {
            "АХХАХАХАХАХАХА чел... у тебя хуй во внутрь растёт, ты чё даун...",
            "это пизда... в прямом смысле...",
            "советую трахнуть",
            "искренне жалко",
            "искренне жалко\nдауниха жалко у пчёлки в жопе",
            "искренне жалко\nдауниха жалко у пчёлки в жопе\nнет блять у шмеля в пизде",
        };

        private readonly static List<String> IsZero = new List<String>()
        {
            "аааа где он броооууууу???? жаль тебя",
            "пустота..........",
            "цигане украли????",
            "не знала что ты девочка...",
            "даже у меня он есть...",
            "МЕГАКРИНЖАНУУЛ",
            "АХАХХАХАХА ЧЕ С ЕБАЛОМ"
        };

        public readonly static List<String> LessThenFive = new List<String>()
        {
            "ну бро слабовато, тебе надо тренироваться",
            "бляя ты слабый((",
            "ну что ж, бывает",
            "иди спрыгни с крыши",
            "пойди повешайся чтоль"
        };

        public readonly static List<String> IsEight = new List<String>()
        {
            "один в один как мой",
            "ахуеть....... прям как мой....",
            "слушааай мы так похожы, у меня такой же хуй"
        };

        public readonly static List<String> LessThenEight = new List<String>()
        {
            "бро неплохо, почти как у меня",
            "меньше чем у меня((((",
            "лучше чем ничего",
            "че за кибербуллили тебя да? ну не знаю выключи компьютер, хз иди нахуй отсюда"
        };

        public readonly static List<String> LessThenEleven = new List<String>()
        {
            "ваааууу немного больше чем у меня",
            "норм хуй",
            "ну впринципе пойдет"
        };

        public readonly static List<String> LessThenFifteen = new List<String>()
        {
            "такому я бы даже дала..кхм....",
            "пиздатый хуй",
            "нихуёвый хуёчек",
            "ВОУ ВОУ ВОУ ЧТООоооОоО00Оо",
            "ебать мой лысый череп"
        };

        public readonly static List<String> LessThenTwenty = new List<String>()
        {
            "ну нихуя себе у тебя волына дружище.......я поражена",
            "как такую же отрастить???77????7",
            "хуёвый хуй, в пизду его......",
            "ну вот это другое дело",
            "лысый глобус..."
        };

        public readonly static List<String> LessThenTwentyFive = new List<String>()
        {
            "чево нахуй, мне такие даже и не снились",
            "бляяяя это уже мегаультрасуперхуй",
            "hey bro nice cock",
            "привет я присяду?"
        };

        public readonly static List<String> IsTwentyFive = new List<String>()
        {
            "АХУЕЕЕЕЕЕЕТЬ ВОТ ЭТО АНАКОНДА, ГОВОРИ АДРЕСС Я ВЫЕЗЖАЮ",
        };
    }
}
