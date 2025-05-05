# LearnQuest 🎮📚

**LearnQuest** is a gamified, demonstration-based learning platform designed to boost learner motivation, engagement, and efficiency. This WebGL-based game combines educational content with RPG mechanics, NPC-led instruction, battle-based evaluations, and real-time feedback.

## 🌐 Play the Game
👉 [Click here to play LearnQuest](https://hesh12388.github.io/learnQuest/)  
*(Best viewed on desktop in Chrome, Firefox, Edge, Brave, or Safari)*

## 🕹️ How to Play

1. **Move Around:** Use `W`, `A`, `S`, `D` or arrow keys to move your character.
2. **Interact with NPCs:** Press `Enter` when near an NPC to begin a lesson.
3. **View Objectives:** Click the objectives icon in the menu to see your tasks.
4. **Fight Creatures:** Click on chasing creatures to answer questions. Correct answers defeat them!
5. **Battle the Boss:** Once all objectives are complete, challenge the level boss.
6. **Complete Achievements:** Complete a variety of course-specific achievements which will test your speed, precision, and accuracy!
7. **Get on the leaderboard:** Earn your way up the leaderboard by completing objectives and achievements!
8. **Customize:** Use the shop and character menu to change appearance and abilities. You can also modify your controls in the settings menu.
9. **Use Tokens:** Activate tokens during battles for hints, extra time, or answer reveals.
10. **Ask for Help:** Talk to the AI Assistant in-game for real-time learning support.
11. **Chat with Other Learners:** Use the in-game chat to talk to other learners and help each other progres. This chat is moderated by AI so make sure your discussion is relevant to learning!

## 🛠️ Technologies Used

- **Unity (WebGL Build):** Game development and deployment  
- **Express.js:** Backend server used to create RESTful API endpoints for retrieving, updating, and storing user data in Firebase Firestore.
- **OpenAI API:** Used to power AI assistant and Moderated-Chat
- **Astra DB (Cassandra):** Used as the vector store for storing and retrieving embedded course documents in the RAG pipeline  
- **Firebase:** Firestore database for storing all learner data 
- **GitHub Pages:** Deployment of the game for public access
- **Render:** Cloud hosting for the backend server

## 📂 Repository Structure

Assets/        → Unity game assets  
Scripts/       → Gameplay logic and event handling, further split into scripts for different systems
Scenes/        → Game levels and world layout, further split into a scene for each level in the game  
docs/    → Exported WebGL game files  

## 📚 References

1. **Wooden Pixel Art GUI UI Kit**  
   Unity Asset Store. Used partially for UI panel design.  
   [https://assetstore.unity.com/packages/2d/gui/wooden-pixel-art-gui-ui-kit-over-750-png-264906](https://assetstore.unity.com/packages/2d/gui/wooden-pixel-art-gui-ui-kit-over-750-png-264906)

2. **Sound Effects and Background Music**  
   - Reddit collection of Pokémon sound effects: [https://www.reddit.com/r/PokemonROMhacks/comments/9xgl7j/pokemon_sound_effects_collection_over_3200_sfx/](https://www.reddit.com/r/PokemonROMhacks/comments/9xgl7j/pokemon_sound_effects_collection_over_3200_sfx/)  
   - Pokémon Game Boy soundtrack (Khinsider): [https://downloads.khinsider.com/game-soundtracks/album/pokemon-game-boy-pok-mon-sound-complete-set-play-cd](https://downloads.khinsider.com/game-soundtracks/album/pokemon-game-boy-pok-mon-sound-complete-set-play-cd)

3. **Battle Backgrounds and UI**  
   GameDevExperiments: Pokémon Tutorial Art Assets (GitHub)  
   [https://github.com/GameDevExperiments/Pokemon-Tutorial-Art-Assets](https://github.com/GameDevExperiments/Pokemon-Tutorial-Art-Assets)

4. **Character and Environment Sprites**  
   DeviantArt – Source for character sprites, enemy sprites, creature spritesheets, tilemaps, and palettes.  
   [https://www.deviantart.com/](https://www.deviantart.com/)

