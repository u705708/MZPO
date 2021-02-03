namespace MZPO.AmoRepo
{
    public interface IModel                                 //Интерфейс модели, к которой можно обращатсья через репозиоторий
    {
#pragma warning disable IDE1006 // Naming Styles
        public static string entityLink { get; }            //Возвращается название ссылки на свою сущность
#pragma warning restore IDE1006 // Naming Styles
    }
}
