using ExplorerOpenGL.Model;
using ExplorerOpenGL.Model.Interface;
using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace ExplorerOpenGL.Managers
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
            Dictionary<string, string> config = GetConfig();

            foreach (var uc in ucs)
                config[uc.Description] = uc.ToConfigFile();

            WriteConfigFile(config);
        }

        public static void SaveConstant(IUserControl uc)
        {
            Dictionary<string, string> config = GetConfig();
            config[uc.Description] = uc.ToConfigFile(); 
            WriteConfigFile(config);
        }

        public static void SaveConstant(string PropertyName, string Value)
        {
            Dictionary<string, string> config = GetConfig();
            config[PropertyName] = Value; 
            WriteConfigFile(config);
        }
        private static void WriteConfigFile(Dictionary<string, string> config)
        {
            using (StreamWriter sw = new StreamWriter("config.ini", false))
            {
                foreach (var uc in config)
                {
                    string propName = uc.Key;
                    string propValue = uc.Value;

                    sw.WriteLine($"{propName}={propValue}");
                }
            }
            Init();
        }

        public static Dictionary<string, string> GetConfig()
        {
            string[] configEntries = File.ReadAllText("config.ini").Split("\n");
            Dictionary<string, string> output = new Dictionary<string, string>();
            foreach (string entry in configEntries)
            {
                if(string.IsNullOrWhiteSpace(entry)) 
                    continue;
                string[] keyValuePaire = entry.Trim().Split("=");
                if (keyValuePaire.Length != 2)
                    continue; 

                output.Add(keyValuePaire[0], keyValuePaire[1]); 
            }
            return output;
        }

        public static Sprite[] GetConstantEditor()
        {
            var props = typeof(ConstantManager).GetProperties();
            List<Sprite> sprites = new List<Sprite>();
            foreach (var prop in props)
            {
                if (prop.Name == "Assertions")
                    continue;

                Constant constant = (prop.GetValue(prop, null) as Constant);

                if (constant.ConstantType == ConstantType.AutoComp)
                    continue; 

                TextZone tz = new TextZone(constant.Description, FontManager.GetFont("Default"), Color.Black);
                IUserControl tb = new TextinputBox(TextureManager.DefaultTextInputTexture);

                if (constant.Type.Name == "String[]")
                    (tb as TextinputBox).Text = string.Join(";", constant.Value as string[]);
                else if (constant.Type.Name == "Boolean")
                {
                    tb = new CheckBox(TextureManager.LoadTexture("UnCheck"), TextureManager.LoadTexture("Check"));
                    (tb as CheckBox).IsCheck = constant.GetValue<bool>();
                }
                else
                    (tb as TextinputBox).Text = constant.Value.ToString();

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
        public static Constant LOGIN_NAME { get; set; } = new Constant(typeof(string), "", "Username :", ConstantType.Both);
        public static Constant FULLSCREEN { get; set; } = new Constant(typeof(bool), false, "Is full screen :");
        public static Constant VSYNC { get; set; } = new Constant(typeof(bool), false, "Vsync :");
        public static Constant LOGIN_HOST { get; set; } = new Constant(typeof(string), "", "", ConstantType.AutoComp);
         

        public static Dictionary<string, Func<IUserControl, AssertResult>> Assertions { get; private set; } = new Dictionary<string, Func<IUserControl, AssertResult>>()
        {
            { "WIDTH", (uc) => { int intout = -1;  int.TryParse((string)uc.GetValueOfUC(), out intout); return new AssertResult(intout >= 800, "Width need to be a number and at least 600"); } },
            { "HEIGHT", (uc) => { int intout = -1;  int.TryParse((string)uc.GetValueOfUC(), out intout); return new AssertResult(intout >= 600, "Height need to be a number and at least 800"); } },
            { "NAME", (uc) => { return new AssertResult(!string.IsNullOrWhiteSpace(uc.GetValueOfUC().ToString()), "Your name can't be empty"); } },
        };
    }
}
