﻿using ExplorerOpenGL.Model.Sprites;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace ExplorerOpenGL.View
{
    public class Camera
    {
        public Matrix Transform { get; set; }
        public Vector2 Bounds { get; private set; }
        public Sprite SpriteToFollow { get; private set; }
        public bool IsFollowingSprite { get; private set; }
        public Vector2 Position { get; private set; }
        public Vector2 LookAtPosition { get; private set; }
        public Camera(Vector2 bounds)
        {
            this.Bounds = bounds; 
        }

        public void Update()
        {
            
            Matrix position;
            if (IsFollowingSprite)
            {
                position = Matrix.CreateTranslation(
                    -SpriteToFollow.Position.X,
                    -SpriteToFollow.Position.Y,
                    0);
                Position = new Vector2(SpriteToFollow.Position.X, SpriteToFollow.Position.Y);
            }
            else
            {
                position = Matrix.CreateTranslation(
                    -LookAtPosition.X,
                    -LookAtPosition.Y,
                    0);
                Position = new Vector2(LookAtPosition.X, LookAtPosition.Y);

            }

            Matrix offset = Matrix.CreateTranslation(
                    Bounds.X/ 2,
                    Bounds.Y / 2,
                    0);

            Transform = position *  offset;
        }

        public void FollowSprite(Sprite target)
        {
            SpriteToFollow = target;
        }

        public void LookAt( Vector2 value)
        {
            LookAtPosition = value;
        }
        public void LookAt(int x, int y)
        {
            LookAtPosition = new Vector2(x, y);
        }

        public void ToggleFollow(bool value)
        {
            IsFollowingSprite = value; 
        }
        public void ToggleFollow()
        {
            IsFollowingSprite = !IsFollowingSprite;
        }


    }
}
