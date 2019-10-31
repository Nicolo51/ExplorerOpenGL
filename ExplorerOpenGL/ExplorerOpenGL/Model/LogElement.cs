using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model
{
    public class LogElement
    {
        public string Text { get; set; }
        public float opacity { get; set; }
        public bool IsRemove { get; set; }
        private float velocity; 
        public LogElement(string Text)
        {
            opacity = 10f;
            IsRemove = false;
            this.Text = Text;
            velocity = 0.1f;
        }

        public void Update()
        {
            if(opacity < 0)
            {
                IsRemove = true; 
            }
            opacity -= velocity; 
        }
    }
}
