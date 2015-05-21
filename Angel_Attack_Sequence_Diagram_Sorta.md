This is mainly for me to document the function calling flow of the different objects we've created in Angel Attacks. If it's useful good, else just ignore it.

# Things to keep in mind #
  * Most classes that are moving "Sprite" derived objects use the "Update()".
  * XNA calls it's own "Game.Update()" our "Game1.Update()" in a continuous loop.
  * Functions listed will be ones created in Angel Attack only.

# Classes using Update() #
  * # Sprite #
| **Updates Functions** | **Uses this(1)** |
|:----------------------|:-----------------|
| **createBounds()**    | Nothing          |

  * # Bullet #
| **Updates Functions** | **Uses this(1)** | **Uses this(2)** | **Uses this(3)** | **Uses this(4)** |
|:----------------------|:-----------------|:-----------------|:-----------------|:-----------------|
| **Sprite.createBounds()** | Nothing          | Nothing          | Nothing          | Nothing          |
| **updateMove()**      | Nothing          | Nothing          | Nothing          | Nothing          |
| **handleCollisions()** | ?Sprite.takeDamage()| ?LesserDemon.takeDamage() | ?Protectee.takeDamge()| Sprite.IntersectPixels() |
| **Sprite.Update()**   | createBounds()   | Nothing          | Nothing          | Nothing          |

  * # Grenadier #
| **Updates Functions** | **Uses this(1)** | **Uses this(2)** |
|:----------------------|:-----------------|:-----------------|
| **Sprite.Update()**   | createBounds()   |
| **handleInput()**     | Sprite.IntersectPixels() | PlayAnimation()  |
| **UpdateAnimation()** | Nothing          | Nothing          |
| **updateAttack()**    | Nothing          | Nothing          |
| **applyPhysics()**    | getCollisions()  | handleCollisions() |
  * # LesserDemon #
  * # Spawner #

# Classes not using Update() #
  * # Text #
  * # Wall #
  * # Protectee #
  * # Block #

Add your content here.  Format your content with:
  * Text in **bold** or _italic_
  * Headings, paragraphs, and lists
  * Automatic links to other wiki pages