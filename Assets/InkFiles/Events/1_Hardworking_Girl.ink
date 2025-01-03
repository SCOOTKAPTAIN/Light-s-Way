INCLUDE ..\globals.ink
/// Good Ending
You come across a small settlement, even when chaos ensues, people are still able to survive together by building communities.#text:left #layout:remove

As you walk past the settlement, you see a group of people moving supplies, working hard as their bodies sweat and tire from heavy work.

While you are observing their work, one of the workers seems to be struggling managing provisions, he looks tired and could pass out any moment from carrying those boxes.

From the looks of it, you are able to help by carrying some weights for him and reduce his workload.

You decide on what to do...
 +[Lend him a hand.]
 ->help1
 
 +[You have better things to do.]
 ->leave1
 
 === leave1 ===
 You are too delicate for such physical work, you prayed for the man's well-being and went on your way. (Light +5)
->END

=== leave2 ===
It looks like you've done everything you can, the man is grateful for you help and bids you farewell.
->END

=== help1 ===
You approached him and offer some help, at first, the man is hesistant to recieve help from such a young girl but you insisted and he decides to give you work moving a few light boxes. (Proficiency +1 \| Max Health -5)

You finished moving everything, and while it's light, it definitely put some strain in you, the man appriciates your help but his work is far from over.

+[Offer to help him more.]
->help2

+[My work here is done.]
->leave2


=== help2 ===

->END









