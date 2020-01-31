using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using RimWorld;

namespace RPG_Inventory_Remake_Common
{
    /// <summary>
    /// Keep track of space usage in the Gear Tab and return the next available rect for drawing
    /// </summary>
    public class SmartRect
    {
        public float x_leftEdge;
        public float x_rightEdge;
        public float x_leftCurPosition;
        public float x_rightCurPosition;
        public float WidthGap;
        public float HeightGap;
        public BodyPartGroupDef BodyPartGroup;
        public Direction LastDirection;
        public SmartRect PreviousSibling;
        public SmartRect NextSibling;
        public List<SmartRect> List;

        public Rect Rect;

        public enum Direction : uint
        {
            Left = uint.MinValue,
            Right = uint.MaxValue
        }

        public SmartRect(Rect rect, BodyPartGroupDef bodyPartGroup,
                         float x_leftCurPosition, float x_rightCurPosition,
                         List<SmartRect> list = null, float x_leftEdge = 0, float x_rightEdge = 0,
                         float widthGap = 10, float heightGap = 10,
                         Direction lastDirection = Direction.Right)
        {
            Rect = rect;
            BodyPartGroup = bodyPartGroup;
            List = list;
            this.x_leftEdge = x_leftEdge;
            this.x_rightEdge = x_rightEdge;
            this.x_leftCurPosition = x_leftCurPosition;
            this.x_rightCurPosition = x_rightCurPosition;
            WidthGap = widthGap;
            HeightGap = heightGap;
            LastDirection = lastDirection;

            PreviousSibling = null;
            NextSibling = null;

            if (List != null)
            {
                List.Add(this);
            }
        }

        #region Functions marsquerading as fields (evil invention, but I use it anyway)
        // Check before using Rimworld source code, many getters are not as innocent as the ones here
        public float y
        {
            get
            {
                return Rect.y;
            }
            set
            {
                Rect.y = value;
            }
        }

        public float width
        {
            get
            {
                return Rect.width;
            }
            set
            {
                Rect.width = value;
            }
        }

        public float height
        {
            get
            {
                return Rect.height;
            }
            set
            {
                Rect.height = value;
            }
        }

        public float yMax
        {
            get
            {
                return Rect.yMax;
            }
        }
        #endregion

        public Rect NextAvailableRect()
        {
            // Return the first rect on the row
            if (x_leftCurPosition == x_rightCurPosition)
            {
                x_rightCurPosition += width;
                return new Rect(x_leftCurPosition, y, width, height);
            }
            LastDirection = ~LastDirection;
            return NextAvailableRect(LastDirection);
        }


        public Rect NextAvailableRect(Direction direction)
        {
            if (direction == Direction.Left)
            {
                if (x_leftEdge < x_leftCurPosition - WidthGap - width)
                {
                    x_leftCurPosition -= (WidthGap + width);
                    return new Rect(x_leftCurPosition, y, width, height);
                }
                //Log.Warning("Failed to draw rect on the left.");
            }

            if (x_rightEdge > x_rightCurPosition + WidthGap + width)
            {
                float x_temp = x_rightCurPosition + WidthGap;
                x_rightCurPosition += (WidthGap + width);
                return new Rect(x_temp, y, width, height);
            }
            //Log.Warning("Failed to draw rect on the right.");
            return default;
        }

        public SmartRect CreateSibling(BodyPartGroupDef bodyPartGroup)
        {
            return CreateSibling(bodyPartGroup, 0, 0);
        }

        public SmartRect CreateSibling(BodyPartGroupDef bodyPartGroup, float leftCurPosition, float rightCurPosition)
        {
            Rect newRect = new Rect(Rect);
            newRect.y += (HeightGap + height);
            return new SmartRect(newRect, bodyPartGroup, leftCurPosition, rightCurPosition,
                                 List, x_leftEdge, x_rightEdge);
        }
    }
}
