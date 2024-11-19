INCLUDE ../globals.ink

... #text:center #layout:remove

In a dark corridor, inside the cold interior of a house, you lie there, unable to move.

Your house has been raided and at the cost of their lives, your family hid you away from them.

Now all there's left is the darkness approaching to consume you.

You've given up on life and the darkness looms ever closer, you've lost the will to stand up, only waiting for your inevitable death.

As the fear for the darkness grows stronger, you ponder on your past life, wondering what kind of life you could have lived.
->background


=== background ===
You reminisce about your aspiration. What kind of person you have dreamed of becoming? #text:left
//A scholar, pursuing of all kinds of knowledge and research.
 + [A scholar.]
     ~(SE("select"))
    ->dream("Scholar")
// An adventurer, exploring everything the world has to offer.
 + [An adventurer.]
    ~(SE("select"))
     ->dream("Adventurer")
//A doctor, offering an helping hand to those in need.
 + [A doctor.]
     ~(SE("select"))
    ->dream("Doctor")
    

===dream(job)===
~ dream_choice = job
{dream_choice == "Scholar": Your yearning for knowledge has left you curious about everything, and you'll never rest until your understanding of the world is complete. (Start with +1 Action Point)}

{dream_choice == "Adventurer": Your resolve has been sharpened by your spirit, making you confident in challenging all sorts of foes in your path. (Start with +5 Attack)}


{dream_choice == "Doctor": Your dedication to help people is a noble cause, though fixtated on healing others, you always find the time to take care of yourself. (Start with +25 Health)}
->never


===never===
But you never did such things, because you.. 

+[Found other things to enjoy.]
~(SE("select"))
->why("found")

+[Are too scared to take a single step.]
~(SE("select"))
->why("scared")

+[Have to stay and take care of your family.]
~(SE("select"))
->why("stay")


===why(reason)===
~reason_choice = reason

{reason_choice == "found": You keep finding things far more interesting, and in the end, that dream of yours is nothing but a distant memory. (Gain an extra card draw) }

{reason_choice == "scared": You never went to start anything, you're too afraid to make a change, in your eyes, everything is fine the way it was. (Gain the opportunity to burn a card) }

{reason_choice == "stay": Your family is struggling so you do what you can to provide, but now they are gone and although wealthy, these coins have lost their purpose. (Gain 200 extra Gold)}
->continue

===continue===
...#text:center
Are you certain those are your past?
+[No, there's something wrong...]
~(SE("select"))
   ->background

+[Yes, I'm sure.]
~(SE("select"))
   ->b

===b===
...
As you finished reminiscing your past, you close your eyes and prepare for your imminent death.
~Music("stop")
...
...
"No."
~Music("intro")
~Background("intro_2")
"I don't like this ending."
You hear a faint voice.
Under all that despair, the voice hidden deep inside you resonates. The voice of the remaining hope you have left. The fragile reminder of your life and the flickering flames of your will to survive.
Yes...
There's still hope.
There's still a way.
There's no way you will end it like this.
~Background("black")
So you stand up, and take the lantern.
~Background("intro_3")
And start you journey, to the sanctuary.
~Music("stop")
~Effect("memoryflash")
~SE("startjourney")








->END


















