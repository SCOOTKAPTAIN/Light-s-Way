INCLUDE ../globals.ink


{ start_choice == "": -> main | -> already_chose }

=== main ===
Which number do you pick?
     + [1]
       -> chosen("one")
     + [2]
       -> chosen("two")
     + [3]
       -> chosen("three")
       
=== chosen(number) ===
~ start_choice = number
You picked {number}.
-> main

=== already_chose ===
You already chose {start_choice}!
-> END