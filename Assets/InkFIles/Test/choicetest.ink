INCLUDE ../globals.ink


What do you want? #layout:remove
     + [Heal]
       -> HealthChange
     + [Proficiency]
       -> ProficiencyChange
     + [Gold]
       -> GoldChange
       

=== ProficiencyChange ===
You are more skilled.
~Proficiency(2)
-> END

=== HealthChange ===
Your health is better now.
~MaxHealth(25)
-> END

=== GoldChange ===
You are richer.
~Gold(100)
->END

// === chosen(number) ===
// ~ dream_choice = number
// You picked {number}.
// "..."#layout:add #speaker:Narrator
// "Hmm..."
// Oh hello.
// It seems that you have done choosing the number.
// Let's see...
// Picked {dream_choice} eh? A good pick.
// Well now, can't have you waiting any longer eh?
// The test is done. Everything went well. Good job programmer!
// -> END




