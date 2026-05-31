using ExplorerOpenGL.Model.Interface;
using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.Model
{
    public class ConstantManager
    {
        public static void Init()
        {
            if (!File.Exists("config.ini"))
                File.Create("config.ini").Close();
            var conf = File.ReadAllText("config.ini").Split('\n');
            foreach (string s in conf)
            {
                if (string.IsNullOrWhiteSpace(s))
                    continue;
                string field = s.Split('=')[0];
                string value = string.Join("=", s.Split('='), 1, s.Split('=').Length - 1).Trim();

                if (field == "Environement")
                    continue;
                try
                {
                    PropertyInfo property = typeof(ConstantManager).GetProperty(field);
                    if (property == null)
                    {
                        MessageBoxIG.Show("Enable to locate " + field + " in the property of the game.", "Error", MessageBoxIGType.Ok);
                        continue;
                    }
                    else
                    {
                        var prop = typeof(ConstantManager).GetProperty(field);
                        Constant obj = (Constant)prop.GetValue(prop, null);
                        obj.Set(value); 
                        //prop.GetValue(prop, null);
                        //property.SetValue(null, new Constant(t, Convert.ChangeType(value, t), desc));
                    }
                }
                catch (Exception e)
                {
                    //MessageBoxIG.Show($"Enable to set {field} to {value} : {e.Message}", "Error", MessageBoxIGType.Ok);
                }
            }
            //DatabaseManager.Init(BDD_UR_CO_STRING);
        }
        public static void SaveConstants(IUserControl[] ucs)
        {
            using (StreamWriter sw = new StreamWriter("config.ini", false))
            {
                foreach (var uc in ucs)
                {
                    string propName = uc.Description;
                    string propValue = uc.ToConfigFile(); 

                    sw.WriteLine($"{propName}={propValue}");
                }
            }

            Init();
        }

        public static Sprite[] GetConstantEditor()
        {
            var props = typeof(ConstantManager).GetProperties();
            List<Sprite> sprites = new List<Sprite>();
            foreach (var prop in props)
            {
                if (prop.Name == "Assertions")
                    continue; 
                TextZone tz = new TextZone((prop.GetValue(prop, null) as Constant).Description, FontManager.GetFont("Default"), Color.Black);
                IUserControl tb = new TextinputBox(TextureManager.DefaultTextInputTexture);

                if ((prop.GetValue(prop, null) as Constant).Type.Name == "String[]")
                    (tb as TextinputBox).Text = string.Join(";", ((prop.GetValue(prop, null) as Constant).Value as string[]));
                else if ((prop.GetValue(prop, null) as Constant).Type.Name == "Boolean")
                {
                    tb = new CheckBox(TextureManager.LoadTexture("UnCheck"), TextureManager.LoadTexture("Check"));
                    (tb as CheckBox).IsCheck = (prop.GetValue(prop, null) as Constant).GetValue<bool>();
                }
                else
                    (tb as TextinputBox).Text = (prop.GetValue(prop, null) as Constant).Value.ToString();

                tb.Description = prop.Name;
                if(Assertions.ContainsKey(tb.Description))
                    tb.Assert = Assertions[tb.Description]; 
                sprites.Add(tz);
                sprites.Add(tb as Sprite);
            }
            return sprites.ToArray();
        }
        public static Constant WIDTH { get; set; } = new Constant(typeof(int), 800, "Resolution width :");
        public static Constant HEIGHT { get; set; } = new Constant(typeof(int), 600, "Resolution height :");
        public static Constant NAME { get; set; } = new Constant(typeof(string), "Nicolas", "Default Name :");
        public static Constant FULLSCREEN { get; set; } = new Constant(typeof(bool), false, "Is full screen :");

        public static Dictionary<string, Func<IUserControl, AssertResult>> Assertions { get; private set; } = new Dictionary<string, Func<IUserControl, AssertResult>>()
        {
            { "WIDTH", (IUserControl uc) => { int intout = -1;  Int32.TryParse((string)uc.GetValueOfUC(), out intout); return new AssertResult(intout >= 800, "Width need to be a number and at least 600"); } },
            { "HEIGHT", (IUserControl uc) => { int intout = -1;  Int32.TryParse((string)uc.GetValueOfUC(), out intout); return new AssertResult(intout >= 600, "Height need to be a number and at least 800"); } },
            { "NAME", (IUserControl uc) => { return new AssertResult(!string.IsNullOrWhiteSpace(uc.GetValueOfUC().ToString()), "Your name can't be empty"); } },
        };
    }
}
