Rougelike Deckbuilder game

Name = Light's Way

## Tutorial pop-ups

Tutorials are multi-page `ScriptableObject` assets. Create one with
`Assets > Create > Light's Way > Tutorial`, add one or more pages, and assign
each page's title, body text, and optional image.

To add the UI:

1. Create a modal panel under the persistent UI canvas.
2. Add `TutorialPanel` to the panel and assign its title, body, image, page
   counter, navigation buttons, and optional `CanvasGroup`.
3. Add `TutorialTrigger` to the map, combat, or mechanic object that should
   explain the feature, assign the tutorial asset, and call `TryShow()` from
   the relevant event. Enable `Show On Start` only for section-entry tutorials.

`GameManager` creates the persistent `TutorialManager`. A tutorial marked
`Once Per Run` is completed once after the player presses Finish and is eligible
again when a new run initializes. A tutorial marked `Complete Forever` stores
completion in `PlayerPrefs` and will not open again on later runs. Closing before
Finish leaves the tutorial eligible to be shown again.
