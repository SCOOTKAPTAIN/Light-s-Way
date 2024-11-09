INCLUDE ../globals.ink

{ start_choice == "": -> main | -> already_chose }

=== main ===
Which number do you pick?
     + [1]
       -> chosen("1")
     + [2]
       -> chosen("2")
     + [3]
       -> chosen("3")
       
=== chosen(number) ===
~ start_choice = number
You picked {number}.
-> END

=== already_chose ===
You already chose {start_choice}!
-> END