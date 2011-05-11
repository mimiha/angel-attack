using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Runtime.InteropServices;
/*----------------------------------MY SUGGESTIONS-----------------------------------------
  At this point when ever programming we should be using the class view toolbar to view
  what's inside different classes. I think developing will go more smoothly for us this
  way. 
  The Debug.WriteLine() can be used to see what's happening in the program. If not shown
  in your VS environment press ctrl+alt+o when the program is running in debug or just 
  programming. 
  Finally to help with navigating .cs files; ctrl+(M+P)or(M+O) expands or collapses all 
  methods, accessors, comments & possibly others. Moving caret on same line as the name to
  a collapse or expanded {code block} and pressing ctrl+M+M will expand or collapse it.
  
  Well these are my suggestions shortcuts might cause problems with stray characters if a 
  ctrl or alt isn't pressed so WATCH OUT!
*/

/*---------------------------------HOW TO SEE MY CODE--------------------------------------
  Visual Studio likes to collapse blocks of comments. I've put labels above major blocks of 
  code in these formats
  /*
    ------CODE BLOCK LABEL------#*
    -------------END------------#*
    CODE BLOCK LABEL------------#*
    CODE BLOCK LABEL END--------#*
  '#' is a number(1, 2, 3, etc).
  Press CTRL+F. 
  'Quick Replace'
  enter values in 'Find What' using this format "#*"
  enter values in 'Replace With' using this format "#*" + "/"
  click 'Replace All'
  Switch 'Find What' and 'Replace What' to comment things out again. Try deleting '/' from '2*'
  CODE BLOCK LABELS to understand better this effect
 */
namespace guiCreator
{
    // My Typedefs(useful for preventing my fingers from bleeding)
    using CorrectVect = LesserDemon.CorrectionVector2;
    using LL_Spr = LinkedList<Sprite>;
    using LL_LL_Obj = LinkedList<LinkedList<object>>;
    using LL_CorrectVect = LinkedList<LesserDemon.CorrectionVector2>;
    using LL_Obj = LinkedList<object>; 
    using LL_Int = LinkedList<int>; 
    using GG_LevelData = GuiGame.LevelData; 

    public class GuiLinkManage
    {

        /* No Wall LinkedList actions
           No Text LinkedList actions
           Yes Sprite LinkedList actions in; Update. Nothing done to linkedlist itself though
        
           Yes Spawner LinkedList actions in; Update
           Yes GuiGame LinkedList actions in; UpdateGui, SaveGui, LoadGui

           WHAT IS A LINKEDLIST?
           It's a template which provides different functionality for any type
           Programmers can get information a linkedlist like the number of elements in it
           WHAT IS A LINKEDLISTNODE
           A LinkedListNode is a single element in a linkedlist, however it can iterate
           through a LinkedList of the same type by using the .next() or .previous()
           Detect value of a Sprite object
           Common functions done to linkedlists
           Removes Node from a LinkedList object
         */


        /*GUILINKMANAGE FIELDS--------------------------------------------------------------1*
        //-------------------------LINKEDLIST STORAGE---------------------------------------2*
        // The following Lists used in Game1, Spawner, Demon and Block.cs
        LinkedList<Sprite> SpriteStorage;                    // Copy of a Sprite List
        LinkedList<LinkedList<object>> ListObjectStorage;    // Copy of a LinkedList 
                                                             // Object List
        LinkedList<int> IntStorage;                          // Copy of a Numerical List
        LinkedList<CorrectVect> CorrectionStorage;           // Copy of CorrectionVector2
                                                             // List
        GuiGame.LevelData LevelStorage;                      // Copy of LevelData
        LinkedList<object> ObjectStorage;                    // Copy of Object List
        //----------------------------------END---------------------------------------------2*


        /*----------------------------LINKEDLIST COPIES-------------------------------------2*
        Sprite[] SpriteCopy;                    // Sprite value from a LinkedList 
        LinkedList<object>[] ListObjectCopy;    // LinkedList object values from a 
                                                // LinkedList LinkedList object 
        object[] ObjectCopy;                    // Object values from a LinkedList
        int[] IntCopy;                          // Integer values from a LinkedList
        //----------------------------------END---------------------------------------------2*


        /*-------------------------------NODE LOCATIONS-------------------------------------2*
        // To use for searching for Node types to delete
        int[] SpritePositions;          // Sprite(Wall, Block, or other derived classes)
        int[] ListObjectPositions;      // LL<LL<object>>(Usually Int of X,Y positions)
        int[] IntPositions;             // Integer(Arguement Number)
        int[] CorrectionPositions;      // Collision detection?
        int[] LevelPositions;           // For Multiple Levels?
        int[] ObjectPositions;          // Has ListObjectPositions Node(mostly int) type position
        //----------------------------------END---------------------------------------------2*
        //FIELD END-------------------------------------------------------------------------1*/


        /*GUILINKMANAGE FIELD MANIPULATIONS-------------------------------------------------1*
        /*----------------------------------CONSTRUCTOR-------------------------------------2*
        // Constructor #1 No Value if used must use accessors to modify class fields
        GuiLinkManage()
        {
            // These are all Empty LinkedLists and Integers. They are "Storage" fields
            SpriteStorage = null; 
            ListObjectStorage = null;
            IntStorage = null;
            CorrectionStorage = null;
            LevelStorage.args = null;

            ObjectStorage = null; 

            // Array Size will be zero for all. LinkedList Element Copies
            SpriteCopy = null;
            ListObjectCopy = null;
            ObjectCopy = null;
            IntCopy = null; 

            // Positions not needed for every LinkedList is empty
            IntPositions = null;
            LevelPositions = null;
            ListObjectPositions = null;
            CorrectionPositions = null;
            ObjectPositions = null;
            SpritePositions = null;
        }

        // Constructor #2: Three LevelData Values accessor use needed for empty fields
        GuiLinkManage(LinkedList<LinkedList<object>> AnObjectListList, 
                      LinkedList<int> AnIntList,  
                      LinkedList<Sprite> AnSpriteList)
        {
            // Reminder ObjectStorage will never be useable when GuiManager is 1st Initial-
            // lized. This is because some prick created a linkedlist of linkedlist objects
            // Anything else that isn't double linked can be.
            ListObjectStorage = AnObjectListList; 
            IntStorage = AnIntList; 
            SpriteStorage = AnSpriteList;
            CorrectionStorage = new LinkedList<CorrectVect>();
            LevelStorage.data = AnSpriteList;
            LevelStorage.args = AnObjectListList;
            LevelStorage.numArgs = AnIntList;
            ObjectStorage = new LL_Obj();

            // LinkedList Element Copies
            SpriteCopy = new Sprite[AnSpriteList.Count]; 
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[ObjectStorage.Count]; 
            IntCopy = new int [AnIntList.Count];

            //LOCATION ALOCATION==============================================================
            // Must be Big enough to store locations of various Node types
            SpritePositions = new int       // Create Room for SpritePosition Data
            [ListObjectStorage.Count(TypeCount => TypeCount.GetType() == typeof(Sprite))];
            
            IntPositions = new int          // Create Room for IntPosition Data
            [IntStorage.Count(TypeCount => TypeCount.GetType() == typeof(int))];

            ListObjectPositions = new int   // Create Room for LL<ObjectPosition> Data
            [ListObjectStorage.Count(TypeCount => TypeCount.GetType() == typeof
            (LinkedList<object>))];
            
            ObjectPositions = new int       // Create Room for ObjectPosition Data
            [ObjectStorage.Count(TypeCount => TypeCount.GetType() == typeof(object) || 
             TypeCount.GetType() == typeof(int))];
            
            CorrectionPositions = new int   // Create Room for CorrectionPosition Data
            [CorrectionStorage.Count(TypeCount => TypeCount.GetType() == typeof(int))];

            LevelPositions = new int[0];    // Do this when we need a linked list of LevelData
            //===================================END==========================================
        }
        // Constructor #2vr2: Three LevelData Values accessor use needed for empty fields
        GuiLinkManage(LinkedList<int> AnIntList, 
                      LinkedList<LinkedList<object>> AnObjectListList,
                      LinkedList<Sprite> AnSpriteList)
        {
            // Reminder ObjectStorage will never be useable when GuiManager is 1st Initial-
            // lized. This is because some prick created a linkedlist of linkedlist objects
            // Anything else that isn't double linked can be.
            ListObjectStorage = AnObjectListList;
            IntStorage = AnIntList;
            SpriteStorage = AnSpriteList;
            CorrectionStorage = new LinkedList<CorrectVect>();
            LevelStorage.data = AnSpriteList;
            LevelStorage.args = AnObjectListList;
            LevelStorage.numArgs = AnIntList;
            ObjectStorage = new LL_Obj();

            // LinkedList Element Copies
            SpriteCopy = new Sprite[AnSpriteList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[ObjectStorage.Count];
            IntCopy = new int[AnIntList.Count];
        }
        // Constructor #2vr3: Three LevelData Values accessor use needed for empty fields
        GuiLinkManage(LinkedList<Sprite> AnSpriteList,
                      LinkedList<LinkedList<object>> AnObjectListList,
                      LinkedList<int> AnIntList)
        {
            // Reminder ObjectStorage will never be useable when GuiManager is 1st Initial-
            // lized. This is because some prick created a linkedlist of linkedlist objects
            // Anything else that isn't double linked can be.
            ListObjectStorage = AnObjectListList;
            IntStorage = AnIntList;
            SpriteStorage = AnSpriteList;
            CorrectionStorage = new LinkedList<CorrectVect>();
            LevelStorage.data = AnSpriteList;
            LevelStorage.args = AnObjectListList;
            LevelStorage.numArgs = AnIntList;
            ObjectStorage = new LL_Obj();

            // LinkedList Element Copies
            SpriteCopy = new Sprite[AnSpriteList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[ObjectStorage.Count];
            IntCopy = new int[AnIntList.Count];
        }
        // Constructor #2vr4: Three LevelData Values accessor use needed for empty fields
        GuiLinkManage(LinkedList<LinkedList<object>> AnObjectListList, 
                      LinkedList<Sprite> AnSpriteList,
                      LinkedList<int> AnIntList)
        {
            // Reminder ObjectStorage will never be useable when GuiManager is 1st Initial-
            // lized. This is because some prick created a linkedlist of linkedlist objects
            // Anything else that isn't double linked can be.
            ListObjectStorage = AnObjectListList;
            IntStorage = AnIntList;
            SpriteStorage = AnSpriteList;
            CorrectionStorage = new LinkedList<CorrectVect>();
            LevelStorage.data = AnSpriteList;
            LevelStorage.args = AnObjectListList;
            LevelStorage.numArgs = AnIntList;
            ObjectStorage = new LL_Obj();

            // LinkedList Element Copies
            SpriteCopy = new Sprite[AnSpriteList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[ObjectStorage.Count];
            IntCopy = new int[AnIntList.Count];
        }
        // Constructor #2vr5: Three LevelData Values accessor use needed for empty fields
        GuiLinkManage(LinkedList<Sprite> AnSpriteList,
                      LinkedList<int> AnIntList,
                      LinkedList<LinkedList<object>> AnObjectListList)
        {
            // Reminder ObjectStorage will never be useable when GuiManager is 1st Initial-
            // lized. This is because some prick created a linkedlist of linkedlist objects
            // Anything else that isn't double linked can be.
            ListObjectStorage = AnObjectListList;
            IntStorage = AnIntList;
            SpriteStorage = AnSpriteList;
            CorrectionStorage = new LinkedList<CorrectVect>();
            LevelStorage.data = AnSpriteList;
            LevelStorage.args = AnObjectListList;
            LevelStorage.numArgs = AnIntList;
            ObjectStorage = new LL_Obj();

            // LinkedList Element Copies
            SpriteCopy = new Sprite[AnSpriteList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[ObjectStorage.Count];
            IntCopy = new int[AnIntList.Count];
        }
        // Constructor #2vr6: Three LevelData Values accessor use needed for empty fields
        GuiLinkManage(LinkedList<int> AnIntList,
                      LinkedList<Sprite> AnSpriteList,
                      LinkedList<LinkedList<object>> AnObjectListList)
        {
            // Reminder ObjectStorage will never be useable when GuiManager is 1st Initial-
            // lized. This is because some prick created a linkedlist of linkedlist objects
            // Anything else that isn't double linked can be.
            ListObjectStorage = AnObjectListList;
            IntStorage = AnIntList;
            SpriteStorage = AnSpriteList;
            CorrectionStorage = new LinkedList<CorrectVect>();
            LevelStorage.data = AnSpriteList;
            LevelStorage.args = AnObjectListList;
            LevelStorage.numArgs = AnIntList;
            ObjectStorage = new LL_Obj();

            // LinkedList Element Copies
            SpriteCopy = new Sprite[AnSpriteList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[ObjectStorage.Count];
            IntCopy = new int[AnIntList.Count];
        }
        //_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
        //_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
        // Constructor #3: LevelData Initializer
        GuiLinkManage(GG_LevelData AnLevel)
        {
            // Reminder ObjectStorage will never be useable when GuiManager is 1st Initial-
            // lized. This is because some prick created a linkedlist of linkedlist objects
            // Anything else that isn't double linked can be.
            ListObjectStorage = AnLevel.args;
            IntStorage = AnLevel.numArgs;
            SpriteStorage = AnLevel.data;
            CorrectionStorage = new LinkedList<CorrectVect>();
            LevelStorage.data = AnLevel.data; ;
            LevelStorage.args = AnLevel.args; ;
            LevelStorage.numArgs = AnLevel.numArgs; ;
            ObjectStorage = new LL_Obj();

            // LinkedList Element Copies
            SpriteCopy = new Sprite[AnLevel.data.Count];
            ListObjectCopy = new LinkedList<object>[AnLevel.args.Count];
            ObjectCopy = new object[ObjectStorage.Count];
            IntCopy = new int[AnLevel.numArgs.Count];
        }
        //_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
        //_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-_-
        // Constructor #4: All Initializer one specific sequence; think SILO(Sprite, Int,
        // LL<LinkedList<object>>, object). Otherwise LL_CorrectVect and LevelData can be 
        // any order
        GuiLinkManage(LL_CorrectVect AnCorrectionVector2List, 
                      LL_Spr AnSpriteList, 
                      LL_Int AnIntList, 
                      LL_LL_Obj AnObjectListList, 
                      LL_Obj AnObjectList, 
                      GG_LevelData AnLevel)
        {
            // Storage for various LinkedLists and POD
            CorrectionStorage = AnCorrectionVector2List;
            SpriteStorage = AnSpriteList;
            IntStorage = AnIntList;
            ListObjectStorage = AnObjectListList;
            ObjectStorage = AnObjectList;
            LevelStorage = AnLevel; 

            // Copies of Element from a Linked List(Don't think I need a LevelData Copy)
            SpriteCopy = new Sprite[AnSpriteList.Count];
            IntCopy = new int[AnIntList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[AnObjectList.Count]; 
        }
        // Constructor #4vr2: All Initializer
        GuiLinkManage(GG_LevelData AnLevel,                       
                      LL_Spr AnSpriteList, 
                      LL_Int AnIntList, 
                      LL_LL_Obj AnObjectListList, 
                      LL_Obj AnObjectList,  
                      LL_CorrectVect AnCorrectionVector2List)
        {
            // Storage for various LinkedLists and POD
            CorrectionStorage = AnCorrectionVector2List;
            SpriteStorage = AnSpriteList;
            IntStorage = AnIntList;
            ListObjectStorage = AnObjectListList;
            ObjectStorage = AnObjectList;
            LevelStorage = AnLevel;

            // Copies of Element from a Linked List(Don't think I need a LevelData Copy)
            SpriteCopy = new Sprite[AnSpriteList.Count];
            IntCopy = new int[AnIntList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[AnObjectList.Count];
        }
        // Constructor #4vr3: All Initializer
        GuiLinkManage(LL_Spr AnSpriteList,                      
                      LL_Int AnIntList, 
                      LL_LL_Obj AnObjectListList, 
                      LL_Obj AnObjectList,  
                      GG_LevelData AnLevel, 
                      LL_CorrectVect AnCorrectionVector2List)
        {
            // Storage for various LinkedLists and POD
            CorrectionStorage = AnCorrectionVector2List;
            SpriteStorage = AnSpriteList;
            IntStorage = AnIntList;
            ListObjectStorage = AnObjectListList;
            ObjectStorage = AnObjectList;
            LevelStorage = AnLevel;

            // Copies of Element from a Linked List(Don't think I need a LevelData Copy)
            SpriteCopy = new Sprite[AnSpriteList.Count];
            IntCopy = new int[AnIntList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[AnObjectList.Count];
        }
        // Constructor #4vr4: All Initializer
        GuiLinkManage(LL_CorrectVect AnCorrectionVector2List, 
                      GG_LevelData AnLevel,
                      LL_Spr AnSpriteList, 
                      LL_Int AnIntList, 
                      LL_LL_Obj AnObjectListList, 
                      LL_Obj AnObjectList)
        {
            // Storage for various LinkedLists and POD
            CorrectionStorage = AnCorrectionVector2List;
            SpriteStorage = AnSpriteList;
            IntStorage = AnIntList;
            ListObjectStorage = AnObjectListList;
            ObjectStorage = AnObjectList;
            LevelStorage = AnLevel;

            // Copies of Element from a Linked List(Don't think I need a LevelData Copy)
            SpriteCopy = new Sprite[AnSpriteList.Count];
            IntCopy = new int[AnIntList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[AnObjectList.Count];
        }
        // Constructor #4vr5: All Initializer
        GuiLinkManage(GG_LevelData AnLevel, 
                      LL_CorrectVect AnCorrectionVector2List,                       
                      LL_Spr AnSpriteList, 
                      LL_Int AnIntList, 
                      LL_LL_Obj AnObjectListList, 
                      LL_Obj AnObjectList)
        {
            // Storage for various LinkedLists and POD
            CorrectionStorage = AnCorrectionVector2List;
            SpriteStorage = AnSpriteList;
            IntStorage = AnIntList;
            ListObjectStorage = AnObjectListList;
            ObjectStorage = AnObjectList;
            LevelStorage = AnLevel;

            // Copies of Element from a Linked List(Don't think I need a LevelData Copy)
            SpriteCopy = new Sprite[AnSpriteList.Count];
            IntCopy = new int[AnIntList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[AnObjectList.Count];
        }
        // Constructor #4vr6: All Initializer
        GuiLinkManage(LL_Spr AnSpriteList,
                      LL_Int AnIntList,
                      LL_LL_Obj AnObjectListList,
                      LL_Obj AnObjectList,
                      LL_CorrectVect AnCorrectionVector2List,
                      GG_LevelData AnLevel)
        {
            // Storage for various LinkedLists and POD
            CorrectionStorage = AnCorrectionVector2List;
            SpriteStorage = AnSpriteList;
            IntStorage = AnIntList;
            ListObjectStorage = AnObjectListList;
            ObjectStorage = AnObjectList;
            LevelStorage = AnLevel;

            // Copies of Element from a Linked List(Don't think I need a LevelData Copy)
            SpriteCopy = new Sprite[AnSpriteList.Count];
            IntCopy = new int[AnIntList.Count];
            ListObjectCopy = new LinkedList<object>[AnObjectListList.Count];
            ObjectCopy = new object[AnObjectList.Count];
        }
        //----------------------------------END---------------------------------------------2*


        /*-----------------------------------ACCESSORS--------------------------------------2*
        //STORAGE ACCESSORS=================================================================
        // LL<Sprite> can be; Bullet, LesserDemon, Protectee, Spawner or Text value
        public LinkedList<Sprite> SPRITESTORAGE 
        {
            get { return SpriteStorage; }
            set { SpriteStorage = value; }
        }
        // LL<LL<object>> has list of arguments for Sprite like IT'S LOCATION or DIRECTION!
        public LinkedList<LinkedList<object>> LISTOBJECTSTORAGE
        {
         get{ return ListObjectStorage; }
         set{ ListObjectStorage = value; }
        }
        // LL<object> has actual argument values for Sprite(See Above)
        public LinkedList<object> OBJECTSTORAGE
        {
         get{ return ObjectStorage; }
         set{ ObjectStorage = value; }
        }
        // LL<int> Number of arguments in LL<object> for use in the save loop, maybe?
        public LinkedList<int> INTSTORAGE
        {
            get{ return IntStorage; }
            set{ IntStorage = value; }
        }
        // LL<CorrectionVector2> So far only useful Wall objects many uses in future
        public LinkedList<CorrectVect> CORRECTIONSTORAGE
        {
            get { return CorrectionStorage; }
            set { CorrectionStorage = value; }
        }
        // LevelData has data on all Sprites and its' derived classes used to draw game
        public GG_LevelData LEVELSTORAGE
        {
            get { return LevelStorage; }
            set { LevelStorage = value; }
        }
        // LEVELDATA ACCESSORS============================================================
        // Simply remember LevelData+(LevelData FieldName) in all caps to use
        // LevelData.numArgs the Sprite's information; location, dimension, etc.
        public LinkedList<int> LEVELDATANUMARGS
        {
            get { return LevelStorage.numArgs; }
            set { LevelStorage.numArgs = value; }
        }
        // LevelData.args a clump of of Sprite arguments
        public LinkedList<LinkedList<object>> LEVELDATAARGS
        {
            get { return LevelStorage.args; }
            set { LevelStorage.args = value; }
        }
        // LevelData.data the Sprite object can be any class derived from it
        public LinkedList<Sprite> LEVELDATADATA
        {
            get { return LevelStorage.data; }
            set { LevelStorage.data = value; }
        }
        //----------------------------------END---------------------------------------------2*
        //GUILINKMANAGER FIELD MANIP END----------------------------------------------------1*/


        /*----------------------------------PLAIN OLD DATA----------------------------------1*
        // A Single Piece in the Level 
        public struct LevelPiece
        {
            public LinkedListNode<LinkedList<object>> ObjectPiece;
            public LinkedListNode<int> IntPiece;
            public LinkedListNode<Sprite> SpritePiece;
        }
        //----------------------------------END---------------------------------------------1*/
        

        /*NODE MANIPULATION FUNCTIONS-------------------------------------------------------1*
        /*-----------------------------------NODE REMOVERS----------------------------------2*
        // Removes Node from LinkedList LinkedList object
        public LinkedList<LinkedList<object>> RemoveElement
            (LinkedList<LinkedList<object>> LinkListObject,
             LinkedListNode<LinkedList<object>> LinkNodeObject)
        {
            LinkListObject.Remove(LinkNodeObject);
            return LinkListObject;
        }

        // Removes Node from LinkedList int
        public LinkedList<int> RemoveElement(LinkedList<int> LinkedListInt,
                                             LinkedListNode<int> LinkNodeInt)
        {
            LinkedListInt.Remove(LinkNodeInt);
            return LinkedListInt;
        }

        // Removes Node from LinkedList Sprite
        public LinkedList<Sprite> RemoveElement(LinkedList<Sprite> LinkedListSprite,
                                                LinkedListNode<Sprite> LinkNodeSprite)
        {
            LinkedListSprite.Remove(LinkNodeSprite);
            return LinkedListSprite;
        }
        // Removes Nodes from LevelData object
        public GuiGame.LevelData RemoveElement(GuiGame.LevelData LevelPlan,
                                               LevelPiece LevelObject)
        {
            LevelPlan.args.Remove(LevelObject.ObjectPiece);
            LevelPlan.data.Remove(LevelObject.SpritePiece);
            LevelPlan.numArgs.Remove(LevelObject.IntPiece);

            return LevelPlan;
        }
        //----------------------------------END---------------------------------------------2*


        /*------------------------------NODE STEPPERS---------------------------------------2*
        // Next Node in object LinkedList
        public LinkedListNode<object> NextElement(LinkedListNode<object> NextObject)
        {
            return NextObject.Next;
        }

        // Next Node in int LinkedList
        public LinkedListNode<int> NextElement(LinkedListNode<int> NextInt)
        {
            return NextInt.Next;
        }

        // Next Node in Sprite LinkedList
        public LinkedListNode<Sprite> NextElement(LinkedListNode<Sprite> NextSprite)
        {
            return NextSprite.Next;
        }

        // Next Piece in Level
        public LevelPiece NextElement(LevelPiece NextPiece)
        {
            NextPiece.ObjectPiece = NextPiece.ObjectPiece.Next;
            NextPiece.IntPiece = NextPiece.IntPiece.Next;
            NextPiece.SpritePiece = NextPiece.SpritePiece.Next;
            return NextPiece;
        }

        // Removes Elements from 
        bool DetectGrenadier(Sprite SpriteValue)  // Confirmation of Grenadier value
        {
            bool GrenadierFlag;
            if (SpriteValue.GetType() == typeof(Grenadier))
                GrenadierFlag = true;
            else
                GrenadierFlag = false;

            return GrenadierFlag;
        }
        //----------------------------------END---------------------------------------------2*


        /*------------------------------FIRST NODE INITIALIZERS-----------------------------2*
        // All of the below Initialize a LinkedListNode of type 'x' to First Node in Linked-
        // List of type 'x' Brief comments on what type 'x' is will be provided
        // X is LinkedList<object> for Node & List
        public LinkedListNode<LinkedList<object>> InitNode
              (LinkedList<LinkedList<object>> ListObjectLinkedList)
        {
            return ListObjectLinkedList.First;
        }
        // X is Sprite 4 Node & List
        public LinkedListNode<Sprite> InitNode(LinkedList<Sprite> SpriteLinkedList)
        {
            return SpriteLinkedList.First;
        }
        // X is int 4 Node & List
        public LinkedListNode<int> InitNode(LinkedList<int> IntLinkedList)
        {
            return IntLinkedList.First;
        }
        // List is GuiGame.LevelData, Node is GuiLinkManage.LevelPiece
        public LevelPiece InitNode(GG_LevelData Level)
        {
            // Stores LevelData's 1st Node
            LevelPiece LevelPieceStorage = new LevelPiece();

            // Getting 1st node of Level
            LevelPieceStorage.IntPiece = Level.numArgs.First;
            LevelPieceStorage.ObjectPiece = Level.args.First;
            LevelPieceStorage.SpritePiece = Level.data.First;

            return LevelPieceStorage;
        }
        //----------------------------------END---------------------------------------------2*
        //NODE MANIPULATION END-------------------------------------------------------------1*/
    }
}
 