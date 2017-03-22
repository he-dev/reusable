using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Reusable.Commands
{
    internal class LinkedCommand : ICommand
    {
        private readonly ICommand _pre;
        private readonly ICommand _post;

        public LinkedCommand(ICommand pre, ICommand post)
        {
            _pre = pre ?? throw new ArgumentNullException(nameof(pre));
            _post = post ?? throw new ArgumentNullException(nameof(post));
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object arg)
        {
            return _pre.CanExecute(arg) && _post.CanExecute(arg);
        }

        public void Execute(object arg)
        {
            _pre.Execute(arg);
            _post.Execute(arg);
        }

        protected void OnCanExecuteChanged()
        {
            CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
