﻿using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    public class LoginScreen : MessageBox
    {
        public const int Height = 250;
        public const int Width = 350;

        TextinputBox tbName;
        TextinputBox tbIP;
        TextZone txtName;
        TextZone txtIP;
        Button btnConnect;
        Button btnBack; 

        public LoginScreen()
            :base()
        {
            SpriteFont font = fontManager.GetFont("Default");
            _texture = textureManager.CreateBorderedTexture(Width, Height, 3, 0, paint => Color.Black, paint => (paint < (Width * 30) ? new Color(22, 59, 224) : new Color(245, 231, 213)));
            SourceRectangle = new Rectangle(0, 0, _texture.Width, _texture.Height);
            Title = "Login in";

            tbName = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black), font);
            tbIP = new TextinputBox(textureManager.CreateTexture(250, 35, paint => Color.Black), font);
            txtName = new TextZone("Your name :", font, Color.Black);
            txtIP = new TextZone("Host address :", font, Color.Black);
            btnConnect = new Button(textureManager.OutlineText("Connect", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Connect", "Default", Color.CornflowerBlue, Color.Black, 2));
            btnBack = new Button(textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 1), textureManager.OutlineText("Back", "Default", Color.CornflowerBlue, Color.Black, 2));
            
            tbName.SetAlignOption(AlignOption.TopLeft);
            tbIP.SetAlignOption(AlignOption.TopLeft);
            txtName.SetAlignOption(AlignOption.TopLeft);
            txtIP.SetAlignOption(AlignOption.TopLeft);
            btnConnect.SetAlignOption(AlignOption.Left);
            btnBack.SetAlignOption(AlignOption.Right);

            btnBack.MouseClicked += BtnBack_MouseClicked;
            btnConnect.MouseClicked += BtnConnect_MouseClicked;
        }

        private void BtnConnect_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            MessageBox.Show("IP :" + tbIP.Text + "\nName : " + tbName.Text);
        }

        private void BtnBack_MouseClicked(object sender, MousePointer mousePointer, Vector2 clickPosition)
        {
            this.Close();
        }

        public override void Show()
        {
            gameManager.AddSprite(this, this);
            AddChildSprite(txtName, new Vector2(50, 50));
            AddChildSprite(tbName, new Vector2(50, 75));
            AddChildSprite(txtIP, new Vector2(50, 125));
            AddChildSprite(tbIP, new Vector2(50, 150));
            AddChildSprite(btnConnect, new Vector2(50, 205));
            AddChildSprite(btnBack, new Vector2(300, 205));
            base.Show();
        }
    }
}