using ExplorerOpenGL.Managers;
using ExplorerOpenGL.Model.Interface;
using ExplorerOpenGL2.Managers;
using ExplorerOpenGL2.Model;
using ExplorerOpenGL2.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ExplorerOpenGL.View
{
    public class OptionScreen : MessageBoxIG
    {
        public const int Width = 350;

        public Button btnApply;
        public Button btnBack;

        public TextZone txtFullScreen;
        public CheckBox cbFullScreen;

        public TextZone txtWidth; 
        public TextZone txtHeight;

        public TextinputBox tbWidth;
        public TextinputBox tbHeight; 


        public OptionScreen()
        {
            SetPosition(new Vector2(GameManager.Width / 2, GameManager.Height / 2));
            Title = "Option";
            SpriteFont font = FontManager.GetFont("Default");

            btnApply = new Button(TextureManager.OutlineText("OK", "Default", Color.Black, Color.White, 2), TextureManager.OutlineText("OK", "Default", Color.Black, Color.White, 4));
            btnBack = new Button(TextureManager.OutlineText("Back", "Default", Color.Black, Color.White, 2), TextureManager.OutlineText("Back", "Default", Color.Black, Color.White, 4));

            var options = ConstantManager.GetConstantEditor();
            int formHeight = 0; 

            for (int i = 0; i < options.Length; i++)
            {
                options[i].SetAlignOption(AlignOptions.Left);

                if (options[i] is CheckBox)
                {
                    AddChildSprite(options[i], new Vector2(250, formHeight));
                    continue;
                }

                formHeight += (i % 2 == 0 ? 50 : 25); 
                AddChildSprite(options[i], new Vector2(50, formHeight));
            }

            formHeight += 100; 

            SetTexture(TextureManager.CreateBorderedTexture(Width, formHeight, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213))));
            SourceRectangle = new Rectangle(0, 0, Texture.Width, Texture.Height);
            
            AddChildSprite(btnApply, new Vector2(50, formHeight -45));
            AddChildSprite(btnBack, new Vector2(300, formHeight - 45));

            //txtWidth = new TextZone("Resolution width :"); 
            //txtHeight = new TextZone("Resolution height :");
            //txtFullScreen = new TextZone("Fullscreen : "); 

            //tbWidth.SetAlignOption(AlignOptions.TopLeft);
            //tbHeight.SetAlignOption(AlignOptions.TopLeft);

            //txtWidth.SetAlignOption(AlignOptions.TopLeft);
            //txtHeight.SetAlignOption(AlignOptions.TopLeft);

            //btnApply.SetAlignOption(AlignOptions.Left);
            //cbFullScreen.SetAlignOption(AlignOptions.Left);
            //txtFullScreen.SetAlignOption(AlignOptions.Left);

            SetAlignOption(AlignOptions.Center);

            btnApply.MouseClicked += BtnApply_MouseClicked;
            btnBack.MouseClicked += BtnBack_MouseClicked;
        }

        private void BtnBack_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            GameManager.ToMainMenu(); 
            this.Close();
        }

        private void BtnApply_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            IUserControl[] ucs = GetUserControls(); 

            foreach (IUserControl uc in ucs) 
            {
                if (uc.Assert == null)
                    continue; 
             
                var ar = uc.Assert(uc);
                if (!ar.Sucess)
                {
                    MessageBoxIG.Show(ar.Message, "Error", MessageBoxIGType.Ok);
                    return;
                }
            }

            ConstantManager.SaveConstants(ucs);
            GameManager.SetViewport(ConstantManager.WIDTH.GetValue<int>(), ConstantManager.HEIGHT.GetValue<int>());
            GameManager.ToggleFullScreen(ConstantManager.FULLSCREEN.GetValue<bool>()); 
            GameManager.ToggleVsync(ConstantManager.VSYNC.GetValue<bool>());
        }

        public override void Show()
        {
            base.Show();
        }
    }
}
