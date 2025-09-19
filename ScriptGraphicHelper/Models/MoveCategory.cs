using System.Collections.ObjectModel;


namespace ScriptGraphicHelper.Models
{
    public class MoveCategory
    {
        public int Hwnd { get; set; }

        public string Title { get; set; }

        public string ClassName { get; set; }

        public string Info { get; set; }
        public ObservableCollection<MoveCategory> Moves { get; set; } = new ObservableCollection<MoveCategory>();

        public MoveCategory(int hwnd, string title, string className, params MoveCategory[] movies)
        {
            this.Hwnd = hwnd;
            this.Title = title;
            this.ClassName = className;
            this.Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
            this.Moves = new ObservableCollection<MoveCategory>(movies);
        }
        public MoveCategory(int hwnd, string title, string className)
        {
            this.Hwnd = hwnd;
            this.Title = title;
            this.ClassName = className;
            this.Info = string.Format("[{0}][{1}][{2}]", hwnd, title, className);
        }
        public MoveCategory()
        {
            this.Hwnd = -1;
            this.Title = "";
            this.ClassName = "";
            this.Info = string.Format("[{0}][{1}][{2}]", this.Hwnd, this.Title, this.ClassName);
            this.Moves = new ObservableCollection<MoveCategory>();
        }
    }
}
