INCLUDE ../globals.ink
~Background("restsite")
You have found a safespace, no harm can come to you here.#text:left #layout:remove
->RestChoice

=== RestChoice ===
What will you do?
   +[Hone Skills]
   ~(SE("select"))
   ~Proficiency(2)
   ->hone
   +[Rest]
   ~(SE("select"))
   ~Health(75)
   ->rest
   +[Pray]
   ~(SE("select"))
   ~Light(25)
   ->Pray
   
   
=== hone ===
The dangers of this world will only grow stronger, so you decided to train yourself and be much prepared for incoming danger.
->END

=== rest===
Taking advantage of the safespace, you decided to take a rest to recover your strength, your journey is still far ahead after all.
->END

=== Pray===
You get on your knees and started praying, you wish to be given enough strength to survive this ordeal.
You start to feel more hopeful and confident and as you finish praying you notice the lantern starts to grow brighter.
->END


   