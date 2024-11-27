INCLUDE ../globals.ink


{ dream_choice == "": -> main | -> already_chose }

=== main ===
Which number do you pick? #layout:remove
     + [1]
       -> chosen("one")
     + [2]
       -> chosen("two")
     + [3]
       -> chosen("three")
       
=== chosen(number) ===
~ dream_choice = number
You picked {number}.
"..."#layout:add #speaker:Narrator
"Hmm..."
Oh hello.
It seems that you have done choosing the number.
Let's see...
Picked {dream_choice} eh? A good pick.
Well now, can't have you waiting any longer eh?
The test is done. Everything went well. Good job programmer!
-> END



=== already_chose ===
You already chose {dream_choice}!
-> END
