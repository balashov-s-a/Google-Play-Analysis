using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace GooglePlay
{
    public class GooglePlayCategories
    {

        public List<string> Apps = new List<string>()
        {
            "Здоровье и спорт"
            , "Персонализация"
            , "Музыка и аудио"
            , "Образование"
            , "Фотография"
            , "Бизнес"
            , "Социальные"
            , "Работа"
            , "Развлечения"
            , "Новости и журналы"
            , "Мультимедиа и видео"
            , "Книги и справочники"
            , "Погода"
            , "Разное"
            , "Спорт"
            , "Финансы"
            , "Стиль жизни"
            , "Путешествия "
            , "Инструменты"
            , "Транспорт"
            , "Медицина"
            , "Связь"
            , "Комиксы"
            , "Покупки"
        };

        public List<string> OldGames = new List<string>()
        {
            //"Аркады и экшн",
            //"Головоломки",
            //"Азартные игры",
            "Другое",
            "Гонки",
            "Спортивные игры"
        };

        // https://support.google.com/googleplay/android-developer/answer/4353443?hl=en
        // "Аркады и экшн" -> "Аркады"
        // "Головоломки" -> "Пазлы"
        // "Азартные игры" -> "Карточные"
        List<string> RenaimedGames = new List<string>()
        {
            "Аркады",
            "Пазлы",
            "Карточные"
        };

        List<string> NewGames = new List<string>()
        {
            "Обучающие"         ,
            "Приключения"       ,
            "Для всей семьи"    ,
            "Настольные игры"   ,
            "Симуляторы"        ,
            "Экшен"             ,
            "Ролевые"           ,
            "Казино"            ,
            "Стратегии"         ,
            "Словесные игры"    ,
            "Викторины"         ,
            "Музыка"
        };

        public List<string> Games;
        public List<string> All;
        public List<string> Old;
        public List<string> New;

        public GooglePlayCategories()
        {
            Games = OldGames.Union(RenaimedGames).Union(NewGames).ToList();
            All = Games.Union(Apps).ToList();

            Old = All.Except(NewGames).ToList();
            New = NewGames.ToList();
        }

        public void VerifyGenres(List<string> genres)
        {
            List<string> newGenres = genres.Except(All).ToList();
            List<string> replacedGenres = All.Except(genres).ToList();

            Debug.Assert(genres.Count() == All.Count);
            All.Sort();
            genres.Sort();
            Debug.Assert(genres.SequenceEqual(All));
        }
    }
}