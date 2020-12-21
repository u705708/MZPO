namespace MZPO.AmoRepo
{
    public interface IModel                                 //Интерфейс модели, к которой можно обращатсья через репозиоторий
    {
        public static string entityLink { get; }            //Возвращается название ссылки на свою сущность
    }
}
