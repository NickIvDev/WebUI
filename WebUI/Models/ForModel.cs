//

namespace WebUI.Models
{
    public abstract class ForModel // абстактый класс для моделей
    {
        // имя (для клиента)
        public string Name { get; set; }

        // имя дирректории
        public string PathName { get; set; }

        // занимаемое место на диске(либо размер диска - для дисков)
        public double SizeElement { get; set; }

        // размерность SizeElement(КБ,МБ,ГБ)
        public string TypeOfSize { get; set; }

        // значение SizeElement в KB(для сортировки списка)
        public double SizeElementToKb { get; set; }

        // ссылка на изображение
        public string Image { get; set; }       

        // доступ к файлу закрыт
        public bool NoAccessToFile { get; set; }        
    }
}
