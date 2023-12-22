using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PvZA11y
{
    enum StoreItem
    {
        PlantGatlingpea,
        PlantTwinsunflower,
        PlantGloomshroom,
        PlantCattail,
        PlantWintermelon,
        PlantGoldMagnet,
        PlantSpikerock,
        PlantCobcannon,
        GameUpgradeImitater,
        Unused_LawnMower,
        ZenMarigold1,
        ZenMarigold2,
        ZenMarigold3,
        ZenGoldWateringcan,
        ZenFertilizer,
        ZenBugSpray,
        ZenPhonograph,
        ZenGardeningGlove,
        ZenMushroomGarden,
        ZenWheelBarrow,
        ZenStinkyTheSnail,
        GameUpgradeSeedSlot,
        GameUpgradePoolCleaner,
        GameUpgradeRoofCleaner,
        GameUpgradeRake,
        ZenAquariumGarden,
        Chocolate,
        ZenTreeOfWisdom,
        ZenTreeFood,
        GameUpgradeFirstaid,
    };

    enum SeedType
    {
        SEED_PEASHOOTER = 0,
        SEED_SUNFLOWER = 1,
        SEED_CHERRYBOMB = 2,
        SEED_WALLNUT = 3,
        SEED_POTATOMINE = 4,
        SEED_SNOWPEA = 5,
        SEED_CHOMPER = 6,
        SEED_REPEATER = 7,
        SEED_PUFFSHROOM = 8,
        SEED_SUNSHROOM = 9,
        SEED_FUMESHROOM = 10,
        SEED_GRAVEBUSTER = 11,
        SEED_HYPNOSHROOM = 12,
        SEED_SCAREDYSHROOM = 13,
        SEED_ICESHROOM = 14,
        SEED_DOOMSHROOM = 15,
        SEED_LILYPAD = 16,
        SEED_SQUASH = 17,
        SEED_THREEPEATER = 18,
        SEED_TANGLEKELP = 19,
        SEED_JALAPENO = 20,
        SEED_SPIKEWEED = 21,
        SEED_TORCHWOOD = 22,
        SEED_TALLNUT = 23,
        SEED_SEASHROOM = 24,
        SEED_PLANTERN = 25,
        SEED_CACTUS = 26,
        SEED_BLOVER = 27,
        SEED_SPLITPEA = 28,
        SEED_STARFRUIT = 29,
        SEED_PUMPKINSHELL = 30,
        SEED_MAGNETSHROOM = 31,
        SEED_CABBAGEPULT = 32,
        SEED_FLOWERPOT = 33,
        SEED_KERNELPULT = 34,
        SEED_INSTANT_COFFEE = 35,
        SEED_GARLIC = 36,
        SEED_UMBRELLA = 37,
        SEED_MARIGOLD = 38,
        SEED_MELONPULT = 39,
        SEED_GATLINGPEA = 40,
        SEED_TWINSUNFLOWER = 41,
        SEED_GLOOMSHROOM = 42,
        SEED_CATTAIL = 43,
        SEED_WINTERMELON = 44,
        SEED_GOLD_MAGNET = 45,
        SEED_SPIKEROCK = 46,
        SEED_COBCANNON = 47,
        SEED_IMITATER = 48,
        SEED_EXPLODE_O_NUT,
        SEED_GIANT_WALLNUT,
        SEED_SPROUT,
        SEED_LEFTPEATER,
        NUM_SEED_TYPES,
        SEED_BEGHOULED_BUTTON_SHUFFLE,
        SEED_BEGHOULED_BUTTON_CRATER,
        SEED_SLOT_MACHINE_SUN,
        SEED_SLOT_MACHINE_DIAMOND,
        SEED_ZOMBIQUARIUM_SNORKLE,
        SEED_ZOMBIQUARIUM_TROPHY,
        SEED_ZOMBIE_NORMAL,
        SEED_ZOMBIE_TRAFFIC_CONE,
        SEED_ZOMBIE_POLEVAULTER,
        SEED_ZOMBIE_PAIL,
        SEED_ZOMBIE_LADDER,
        SEED_ZOMBIE_DIGGER,
        SEED_ZOMBIE_BUNGEE,
        SEED_ZOMBIE_FOOTBALL,
        SEED_ZOMBIE_BALLOON,
        SEED_ZOMBIE_SCREEN_DOOR,
        SEED_ZOMBONI,
        SEED_ZOMBIE_POGO,
        SEED_ZOMBIE_DANCER,
        SEED_ZOMBIE_GARGANTUAR,
        SEED_ZOMBIE_IMP,
        MaxCount,
        NUM_SEEDS_IN_CHOOSER = 49,
        SEED_NONE = -1,
    };

    public static class Consts
    {
        public static int[] plantCosts = new int[] { 100, 50, 150, 50, 25, 175, 150, 200, 0, 25, 75, 75, 75, 25, 75, 125, 25, 50, 325, 25, 125, 100, 175, 125, 0, 25, 125, 100, 125, 125, 125, 100, 100, 25, 100, 75, 50, 100, 50, 300, 250, 150, 150, 225, 200, 50, 125, 500, 0, 0, 0, 0, 200 };
        public static int[] plantCooldowns = new int[] { 750, 750, 5000, 3000, 3000, 750, 750, 750, 750, 750, 750, 750, 3000, 750, 5000, 5000, 750, 3000, 750, 3000, 5000, 750, 750, 3000, 3000, 3000, 750, 750, 750, 750, 3000, 750, 750, 750, 750, 750, 750, 750, 3000, 750, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 5000, 750, 3000, 3000, 3000, 750 };

        public static string[] plantNames = new string[]{
            "Peashooter",
            "Sunflower",
            "Cherry Bomb",
            "Wall-nut",
            "Potato Mine",
            "Snow Pea",
            "Chomper",
            "Repeater",
            "Puff-shroom",
            "Sun-shroom",
            "Fume-shroom",
            "Grave Buster",
            "Hypno-shroom",
            "Scaredy-shroom",
            "Ice-shroom",
            "Doom-shroom",
            "Lily Pad",
            "Squash",
            "Threepeater",
            "Tangle Kelp",
            "Jalapeno",
            "Spike Weed",
            "Torchwood",
            "Tall-nut",
            "Sea-shroom",
            "Plantern",
            "Cactus",
            "Blover",
            "Split Pea",
            "Starfruit",
            "Pumpkin",
            "Magnet-shroom",
            "Cabbage-pult",
            "Flower Pot",
            "Kernel-pult",
            "Coffee Bean",
            "Garlic",
            "Umbrella Leaf",
            "Marigold",
            "Melon-pult",
            "Gatling Pea",
            "Twin Sunflower",
            "Gloom-shroom",
            "Cattail",
            "Winter Melon",
            "Gold Magnet",
            "Spikerock",
            "Cob Cannon",
            "Imitater",
            "Explode-o-nut",
            "Giant Wall-nut",
            "",
            "Backwards Repeater",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
        };

        public static string[] plantDescriptions = new string[]{
            "Shoots peas at the enemy",
            "Gives you additional sun",
            "Blows up all zombies in a 3 by 3 area",
            "Blocks off zombies and protects your other plants",
            "Explodes on contact, but takes time to arm itself. Only harms zombies in the tile it was planted on.",
            "Shoots frozen peas that damage and slow the enemy",
            "Devours a zombie whole, but is vulnerable while chewing",
            "Fires two peas at a time",
            "Shoots short-ranged spores at the enemy, up to 3 tiles away.",
            "Gives small sun at first and normal sun later",
            "Shoots fumes that can pass through screen doors, up to 4 tiles away",
            "Plant it on a grave to remove the grave",
            "Makes a zombie fight for you when eaten",
            "Long-ranged shooter that hides when enemies get near it",
            "Temporarily immobilizes all zombies on the screen",
            "Destroys a large area, leaving a crater in its wake. Can destroy zombies up to 3.5 tiles away.",
            "Lets you plant non-aquatic plants on top of it",
            "Squashes zombies that get close to it",
            "Shoots peas in the current row, the row above it, and the row below.",
            "Aquatic plant that pulls a zombie underwater when touched.",
            "Destroys an entire row of zombies",
            "Pops tires and hurts zombies that step on it",
            "Peas that pass through it turn into fireballs",
            "Heavy-duty wall that can't be vaulted over",
            "Aquatic plant that shoots short-ranged spores, up to 3 tiles away",
            "Lights up an area, letting you see through fog",
            "Shoots spikes that can pop balloons",
            "Blows away all balloon zombies and fog",
            "Shoots peas forward and backwards",
            "Shoots stars in 5 directions. Up, down, left, diagonal up-right and down-right",
            "Protects plants that are within its shell",
            "Removes helmets and other metal objects from zombies",
            "Hurls cabbages at the enemy",
            "Lets you plant on the roof",
            "Flings corn kernels and butter at zombies",
            "Plant it on a mushroom to wake it up",
            "Diverts zombies into other rows",
            "Protects nearby plants from bungees and catapults",
            "Gives you silver and gold coins",
            "Does heavy damage to groups of zombies",
            "Shoots four peas at a time",
            "Gives twice as much sun as a sunflower",
            "Releases heavy fumes in a 3 by 3 area around itself",
            "Attacks any row and shoots down balloon zombies",
            "Does heavy damage and slows groups of zombies",
            "Collects coins and diamonds for you",
            "Pops multiple tires and damages zombies that walk over it",
            "Click to launch deadly cobs of corn",
            "Lets you have two of the same plant",
            "Explodes on contact",
            "Bowls straight over all zombies in its path.",
            "",
            "Repeater that shoots backwards.",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            "",
            ""
        };

        public static string[] plantFullDescriptions = new string[]{
            "Peashooters are your first line of defense. They shoot peas at attacking zombies. Damage: normal. How can a single plant grow and shoot so many peas so quickly? Peashooter says, \"Hard work, commitment, and a healthy, well-balanced breakfast of sunlight and high-fiber carbon dioxide make it all possible.\"",
            "Sunflowers are essential for you to produce extra sun. Try planting as many as you can. Sun production: normal. Sunflower can't resist bouncing to the beat. Which beat is that? Why, the life-giving jazzy rhythm of the Earth itself, thumping at a frequency only Sunflower can hear.",
            "Cherry Bombs can blow up all zombies in an area. They have a short fuse so plant them near zombies. Damage: massive. Range: all zombies in a 3 by 3 area. Usage: single use, instant.\"I wanna explode,\" says Cherry #1. \"No, let's detonate instead!\" says his brother, Cherry #2. After intense consultation they agree to explodonate.",
            "Wall-nuts have hard shells which you can use to protect your other plants. Toughness: high.\"People wonder how I feel about getting constantly chewed on by zombies,\" says Wall-nut. \"What they don't realize is that with my limited senses all I can feel is a kind of tingling, like a relaxing back rub.\"",
            "Potato Mines pack a powerful punch, but they need a while to arm themselves. You should plant them ahead of zombies. They will explode on contact. Damage: massive. Range: all zombies in the current tile. Usage: single use, delayed activation. Some folks say Potato Mine is lazy, that he leaves everything to the last minute. Potato Mine says nothing. He's too busy thinking about his investment strategy.",
            "Snow Peas shoot frozen peas that damage and slow the enemy. Damage: normal, slows zombies. Folks often tell Snow Pea how \"cool\" he is, or exhort him to \"chill out.\" They tell him to \"stay frosty.\" Snow Pea just rolls his eyes. He's heard 'em all.",
            "Chompers can devour a zombie whole, but they are vulnerable while chewing. Damage: massive. Range: very short. Special: long delay between chomps. Chomper almost got a gig doing stunts for The Little Shop of Horrors but it fell through when his agent demanded too much on the front end. Chomper's not resentful, though. He says it's just part of the business.",
            "Repeaters fire two peas at a time. Damage: normal (for each pea. Firing speed: 2x. Repeater is fierce. He's from the streets. He doesn't take attitude from anybody, plant or zombie, and he shoots peas to keep people at a distance. Secretly, though, Repeater yearns for love.",
            "Puff-shrooms are cheap, but can only fire a short distance. Damage: normal. Range: 3 tiles. Sleeps during the day.\"I only recently became aware of the existence of zombies,\" says Puff-shroom. \"Like many fungi, I'd just assumed they were fairy tales or movie monsters. This whole experience has been a huge eye-opener for me.\"",
            "Sun-shrooms give small sun at first and normal sun later. Sun production: low, then normal. Sleeps during the day. Sun-shroom hates sun. He hates it so much that when it builds up in his system, he spits it out as fast as he can. He just won't abide it. To him, sun is crass.",
            "Fume-shrooms shoot fumes that can pass through screen doors. Damage: normal, penetrates screen doors. Range: all zombies in the fume cloud, up to 4 tiles away. Sleeps during the day.\"I was in a dead-end job producing yeast spores for a bakery,\" says Fume-shroom. \"Then Puff-shroom, bless 'im, told me about this great opportunity blasting zombies. Now I really feel like I'm making a difference.\"",
            "Plant Grave Busters on graves to remove the graves. Usage: single use, must be planted on graves. Special: removes graves. Despite Grave Buster's fearsome appearance, he wants everyone to know that he loves kittens and spends his off hours volunteering at a local zombie rehabilitation center. \"It's just the right thing to do,\" he says.",
            "When eaten, Hypno-shrooms will make a zombie turn around and fight for you. Usage: single use, on contact. Special: makes a zombie fight for you. Sleeps during the day. \"Zombies are our friends,\" asserts Hypno-shroom. \"They're badly misunderstood creatures who play a valuable role in our ecology. We can and should do more to bring them round to our way of thinking.\"",
            "Scaredy-shrooms are long-ranged shooters that hide when enemies get near them. Damage: normal. Special: stops shooting when enemy is close. Sleeps during the day.\"Who's there?\" whispers Scaredy-shroom, voice barely audible. \"Go away. I don't want to see anybody. Unless it's the man from the circus.\"",
            "Ice-shrooms temporarily immobilize all zombies on the screen. Damage: very light, immobilizes zombies. Range: all zombies on the screen. Usage: single use, instant. Sleeps during the day. Ice-shroom frowns, not because he's unhappy or because he disapproves, but because of a childhood injury that left his facial nerves paralyzed.",
            "Doom-shrooms destroy everything in a large area and leave a crater that can't be planted on. Damage: massive. Range: all zombies in about a 4 by 4 area. Usage: single use, instant. Special: leaves a crater. Sleeps during the day.\"You're lucky I'm on your side,\" says Doom-shroom. \"I could destroy everything you hold dear. It wouldn't be hard.\"",
            "Lily pads let you plant non-aquatic plants on top of them. Special: non-aquatic plants can be planted on top of it. Must be planted in water. Lily Pad never complains. Lily Pad never wants to know what's going on. Put a plant on top of Lily Pad, he won't say a thing. Does he have startling opinions or shocking secrets? Nobody knows. Lily Pad keeps it all inside.",
            "Squashes will smash the first zombie that gets close to it. Damage: massive. Range: short range, hits all zombies that it lands on. Usage: single use. \"I'm ready!\" yells Squash. \"Let's do it! Put me in! There's nobody better! I'm your guy! C'mon! Whaddya waiting for? I need this!\"",
            "Threepeaters shoot peas in three lanes. Damage: normal (for each pea. Range: three rows. Threepeater likes reading, backgammon and long periods of immobility in the park. Threepeater enjoys going to shows, particularly modern jazz. \"I'm just looking for that special someone,\" he says. Threepeater's favorite number is 5.",
            "Tangle Kelp are aquatic plants that pull the first zombie that nears them underwater. Damage: massive. Usage: single use, on contact. Must be planted in water.\"I'm totally invisible,\" Tangle Kelp thinks to himself. \"I'll hide here just below the surface and nobody will see me.\" His friends tell him they can see him perfectly well, but he'll never change.",
            "Jalapenos destroy an entire lane of zombies. Damage: massive. Range: all zombies in a row. Usage: single use, instant.\"NNNNNGGGGGG!!!!!!!!\" Jalapeno says. He's not going to explode, not this time. But soon. Oh, so soon. It's close. He knows it, he can feel it, his whole life's been leading up to this moment.",
            "Spikeweeds pop tires and hurt any zombies that step on them. Damage: normal. Range: all zombies that walk over it. Special: can't be eaten by zombies. Hockey is Spikeweed's obsession. He's got box seat season tickets. He keeps close track of his favorite players. And he consistently cleans up in the office hockey pool. Just one problem: he's terrified of pucks.",
            "Torchwoods turn peas that pass through them into fireballs that deal twice as much damage. Special: doubles the damage of peas that pass through it.  Fireballs deal damage to nearby zombies on impact. Everybody likes and respects Torchwood. They like him for his integrity, for his steadfast friendship, for his ability to greatly maximize pea damage. But Torchwood has a secret: he can't read.",
            "Tall-nuts are heavy-duty wall plants that can't be vaulted over. Toughness: very high. Special: can't be vaulted or jumped over. People wonder if there's a rivalry between Wall-nut and Tall-nut. Tall-nut laughs a rich, baritone laugh. \"How could there be anything between us? We are brothers. If you knew what Wall-nut has done for me...\" Tall-nut's voice trails off and he smiles knowingly.",
            "Sea-shrooms are aquatic plants that shoot short ranged spores. Damage: normal. Range: short. Must be planted in water. Sleeps during the day. Sea-shroom has never seen the sea. It's in his name, he's heard loads about it, but he's just never found the time. One day, though, it'll go down.",
            "Planterns light up an area, letting you see through fog. Range: 3 by 3 area . Special: lets you see through fog. Plantern defies science. He just does. Other plants eat light and excrete oxygen; Plantern eats darkness and excretes light. Plantern's cagey about how he does it. \"I'm not gonna say 'sorcery,' I wouldn't use the term 'dark forces,' I just... I think I've said enough.\"",
            "Cactuses shoot spikes that can hit both ground and air targets. Damage: normal. Range: ground and air in current row. She's prickly, sure, but Cactus's spikes belie a spongy heart filled with love and goodwill. She just wants to hug and be hugged. Most folks can't hang with that, but Cactus doesn't mind. She's been seeing an armadillo for a while and it really seems to be working out.",
            "Blovers blow away all balloon zombies and fog. Usage: single use, instant. Special: blows away all balloon zombies. When Blover was five he got a shiny new birthday cake. Blover made his wish, huffed and puffed, but was able to extinguish only 60% of the candles. Instead of giving up, though, he's used that early defeat as a catalyst to push himself harder ever since.",
            "Split Peas shoot peas forward and backwards. Damage: normal. Range: forward and backwards. Firing Speed: 1x forward, 2x backwards. \"Yeah, I'm a Gemini,\" says Split Pea. \"I know, big surprise. But having two heads --or really, one head with a large head-like growth on the back-- pays off big in my line of work.\"",
            "Starfruits shoot stars in 5 directions. Damage: normal. Range: 5 directions. Up, Down, Left, Up-Right, and Down-Right. \"Aw, man,\" says Starfruit. \"I went to the dentist the other day and he said I have four cavities. I've got --count it-- ONE tooth! Four cavities in one tooth? How does this happen?\"",
            "Pumpkins protect plants that are within their shells. Toughness: high. Special: can be planted over another plant. Pumpkin hasn't heard from his cousin Renfield lately. Apparently Renfield's a big star, some kind of... what was it... sports hero? Peggle Master? Pumpkin doesn't really get it. He just does his job.",
            "Magnet-shrooms remove helmets and other metal objects from zombies. Range: nearby zombies. Special: removes metal objects from zombies. Sleeps during the day. Magnetism is a powerful force. Very powerful. Sometimes it scares Magnet-shroom a little. He's not sure if he can handle that kind of responsibility.",
            "Cabbage-pults hurl cabbages at the enemy. Damage: normal. Range: lobbed. Cabbage-pult is okay with launching cabbages at zombies. It's what he's paid for, after all, and he's good at it. He just doesn't understand how the zombies get up on the roof in the first place.",
            "Flower Pots let you plant on the roof. Special: allows you to plant on the roof.\"I'm a pot for planting. Yet I'm also a plant. HAS YOUR MIND EXPLODED YET?\"",
            "Kernel-pults fling corn kernels and butter at zombies. Damage: light (kernel), normal (butter. Range: lobbed. Special: butter immobilizes zombies. Kernel-Pult is the eldest of the Pult brothers. Of the three of them, Kernel is the only one who consistently remembers the others' birthdays. He bugs them about it a little, too.",
            "Use Coffee Beans to wake up sleeping mushrooms. Usage: single use, instant. Special: can be planted over another plant, wakes up mushrooms.\"Hey, guys, hey!\" says Coffee Bean. \"Hey! What's up? Who's that? Hey! Didja see that thing? What thing? Whoa! Lions!\" Yep, Coffee Bean sure does get excited.",
            "Garlic diverts zombies into other lanes. Usage: on contact. Special: diverts zombies into other lanes. Lane-diversion isn't just Garlic's profession. It's his passion. He carries an advanced Doctorate in Redirection from the Brussels University. He'll talk all day about lane vectors and repulse arrays. He even pushes things into alternate avenues at home. Somehow his wife puts up with it.",
            "Umbrella Leaves protect nearby plants from bungees and catapults. Special: protects adjacent plants from bungees and catapults.\"SPROING!\" says Umbrella Leaf. \"Didja like that? I can do it again. SPROING! Woo! That's me popping up to protect stuff around me. Yeah. Just like that. EXACTLY like that. Believe it.\"",
            "Marigolds give you silver and gold coins. Special: gives coins. Marigold spends a lot of time deciding whether to spit out a silver coin or a gold one. She thinks about it, weighs the angles. She does solid research and keeps up with current publications. That's how winners stay ahead.",
            "Melon-pults do heavy damage to groups of zombies. Damage: heavy. Range: lobbed. Special: melons damage nearby enemies on impact. There's no false modesty with Melon-pult. \"Sun-for-damage, I deliver the biggest punch on the lawn,\" he says. \"I'm not bragging. Run the numbers. You'll see.\"",
            "Gatling Peas shoot four peas at a time. Damage: normal (for each pea. Firing Speed: 4x. Must be planted on repeaters. Gatling Pea's parents were concerned when he announced his intention to join the military. \"But honey, it's so dangerous!\" they said in unison. Gatling Pea refused to budge. \"Life is dangerous,\" he replied, eyes glinting with steely conviction.",
            "Twin Sunflowers give twice as much sun as a normal sunflower. Sun production: double. Must be planted on sunflowers. It was a crazed night of forbidden science that brought Twin Sunflower into existence. Thunder crashed overhead, strange lights flickered, even the very roaring wind seemed to hiss its angry denial. But to no avail. Twin Sunflower was alive, ALIVE!",
            "Gloom-shrooms release heavy fumes in an area around themselves. Must be planted on fume-shrooms.\"I've always enjoyed releasing heavy fumes,\" says Gloom Shroom. \"I know a lot of people aren't cool with that. They say it's rude or that it smells bad. All I can say is, would you rather have your brain eaten by zombies?\"",
            "Cattails can attack any lane and shoot down balloon zombies too. Must be planted on lily pads.\"Woof!\" says Cattail. \"Woof woof woof! Does this confuse you? Do you expect me to say 'Meow' like a cat just because the word 'cat' is in my name and I also look like a cat? That's not how things work around here. I refuse to be pigeonholed.\"",
            "Winter Melons do heavy damage and slow groups of zombies. Damage: very heavy. Range: lobbed. Firing Speed: 1/2x. Special: melons damage and freeze nearby enemies on impact. Must be planted on melon-pults. Winter Melon tries to calm his nerves. He hears the zombies approach. Will he make it? Will anyone make it?",
            "Gold Magnets collect coins and diamonds for you. Must be planted on magnet-shrooms.\"How did I end up here?\" asks Gold Magnet. \"I was on the fast track --corner office, full benefits, stock options. I was gonna be Vice President of Midwestern Operations. Now I'm here, on this lawn, in serious danger of being eaten to death. Ooh! A coin!\"",
            "Spikerocks pop multiple tires and damage zombies that walk over it. Must be planted on spikeweeds. Spikerock just got back from a trip to Europe. He had a great time, met some wonderful people, really broadened his horizons. He never knew they made museums so big, or put so many paintings in them. That was a big surprise for him.",
            "Click on the Cob Cannon to launch deadly cobs of corn. Must be planted on 2 side-by-side kernel-pults. What's the deal with Cob Cannon, anyway?  He went to Harvard. He practices law in a prestigious New York firm. He can explode whole areas of zombies with a single corn-launch. All this is common knowledge. But deep inside, what really makes him tick?",
            "Imitaters let you use two of the same plant during a level!\"I remember the Zombie Wars back in '76,\" says Imitater in a raspy, old-man's voice. \"Back then, we didn't have all these fancy peashooters and jalapenos. All we had was guts. Guts and a spoon.\""
        };

        public static Dictionary<int, string> TreeDialogue = new Dictionary<int, string>()
        {
            {1, "Thank you for feeding me! Keep giving me food and I'll give you valuable information!" },
            {2, "Chompers and wall-nuts work exceedingly well together. It's no surprise, considering they were roommates in college." },
            {3, "If you ever listen to anything I say, listen to this: you want two columns of sunflowers. I'm dead serious here." },
            {4, "Snorkel zombies. I hate 'em. How do I deal? Wall-nuts on lily pads, that's how." },
            {5, "Pssst! Try typing 'future' while playing to experience zombies... from the FUTURE!" },
            {6, "How many cherry bombs does it take to take down a Gargantuar? Here's a hint: more than one, fewer than three. Here's a more explicit hint: Two." },
            {7, "If you're looking for mushroom plants for your Zen Garden, you'll have better luck playing on levels where it's nighttime." },
            {8, "I wouldn't worry about permanently damaging your lawn with doom-shrooms. In time the earth heals itself." },
            {9, "Have you tried clicking on the flowers on the main menu? Give it a shot! I'll wait here." },
            {10, "Legend has it that frozen zombies eat slower. I'm here to tell that legend has its facts straight." },
            {11, "Have you heard of the elusive Yeti Zombie? Some say he likes hiding where it's pitch black." },
            {12, "What's cheaper than free? Nothing! That's why puff-shrooms are essential on all night levels! " },
            {13, "Are you hoping to find water plants for your Zen Garden? I bet my phloem you'll have the most luck searching in pool levels." },
            {14, "Have you noticed that Gargantuars sometimes use OTHER ZOMBIES to bash your plants?  Whatever works, I guess." },
            {15, "Stinky the Snail sure loves his chocolate. Maybe loves it a little too much, you know? He won't sit still for an hour after he's had some." },
            {16, "If you think playing survival 'endless' mode only drops pool-style plants for your Zen Garden, think again!  It drops everything-style." },
            {17, "Often the question is asked: where do you find chocolate? A better question would be: where DON'T you find chocolate? It drops in every game mode!" },
            {18, "Grave Busters, eh? Pick 'em only when you can see graves on the right side of the screen along with the zombies. That's what I do." },
            {19, "I've heard that Buckethead zombies take five times as many hits as regular ones." },
            {20, "I hear that typing 'mustache' brings about a terrifying transformation in the undead!" },
            {21, "Do multiple Snow-peas in a row slow zombies down more than just one? The sad but truthful answer is 'Nay.'" },
            {22, "You know that zombies emerge from gravestones, right? So what's stopping you from using grave busters to get rid of them in Survival night? Is it pride?" },
            {23, "If you're looking for the inside info on how long a level's going to be, count the flags on the level meter. That'll set you up real nice." },
            {24, "Roof cleaners. Classic items. Can't recommend them highly enough. Best thing about them? They give you a shot at beating Pogo Party." },
            {25, "If you're wondering if feeding a hypno-shroom to a dancer zombie compels him to summon backup dancers for you, bet it all on 'Yes.'" },
            {26, "Make Money Fast! By Playing Survival Endless! Then E-mail Me Your Bank Account Number!" },
            {27, "You'd think torchwoods would douse snow peas. And you'd be correct, because you, my friend, are one smart cookie." },
            {28, "Those hateful ZomBotany zombies! Who do they think they are, shooting at your plants? It's a good thing wall-nuts stop 'em cold." },
            {29, "The Pogo Party and Bobsled Bonanza mini-games are really, really, really difficult. Wanna drop one of the 'reallys' off that description? Use the squash." },
            {30, "Just when you thought jalapenos couldn't be any more useful, a Tree of Wisdom lets you know that they also destroy the Zomboni's ice trails! BAM!" },
            {31, "Once you buy the imitater, try clicking the little drawing in the upper left corner of your Almanac to access the entry on that sucker." },
            {32, "The number of coins you receive in Wall-nut Bowling is proportional to how cool you are as measured by how many ricochets per nut you can pull off." },
            {33, "Please do not tap on the glass! Or actually, go ahead; right-click on your Aquarium Garden or during Zombiquarium to deafen your underwater creatures." },
            {34, "When I was just an acorn my grampa told me, 'Son, Vasebreaker puzzles are much easier if you break the vases on the right side first.' " },
            {35, "Dancers in I, Zombie may seem expensive, but in the right situation they're worth every penny." },
            {36, "I had a dream. In it, cattail spikes popped balloons and dropped zombies to the ground. I don't know what it means." },
            {37, "Growing aquatic plants in your Zen Garden is pretty much impossible without the Aquarium Garden. Just saying." },
            {38, "Digger zombies violate the natural order with their subterranean ways. It's only fair to use magnet-shrooms to steal their mining picks." },
            {39, "Every day brings new challenges and opportunities. Oh, and new marigolds in Crazy Dave's shop." },
            {40, "Mushroom Garden! Huh! What is it good for? Absolutely nothin'! Except growing mushrooms, that is." },
            {41, "Tired? Depressed? Ladders on tall-nuts getting you down? A quick magnet-shroom will whisk your cares away!" },
            {42, "The tallness of tall-nuts earns widespread acclaim due to their effectiveness vs. Dolphin Riders and Pogo Zombies." },
            {43, "The explosive force of a cherry bomb or jalapeno is more than capable of dislodging a ladder from a Wall-nut." },
            {44, "It's tempting to feed all your chocolate to Stinky the Snail. He's such a chocolate hog. But remember: Zen Garden plants like chocolate too!" },
            {45, "Torchwood fire is hotter than rage, but Zombonis, screen doors, ladders and catapults can take the heat." },
            {46, "If you rely on upgrade plants in Survival: Endless, be acutely aware that they get more expensive the more you have on your lawn." },
            {47, "The Imps in I, Zombie seem weak. But they're speedy and great for fetching that last brain when you've cleared the rest of the opposition." },
            {48, "If you type 'trickedout,' don't be surprised if you see something wacky happen to your lawnmowers." },
            {49, "Thank you for feeding me! I'm out of new wisdom for now, but I might have more if you grow me tall enough!" },
            {101, "Mmmm, I could sure use some yummy fertilizer!" },
            {102, "I think I've seen that cloud before." },
            {103, "Don't mind me. I'll just be over here, growing." },
            {104, "I'm metabolizing like crazy!" },
            {105, "I'll never get why you animals spend all day moving around like you do." },
            {106, "I experience time at a vastly slower rate than you!" },
            {107, "I think I'm perennial!" },
            {108, "My xylem is tingling!" },
            {109, "You can get a lot of wisdom just by standing around." },
            {110, "So I've heard about this \"winter\" dealie. Can't say I'm looking forward to it." },
            {201, "Mmmm... sunlight is DELICIOUS!" },
            {202, "Oops, sorry... I just gave off some oxygen." },
            {203, "Gosh, I can grow leaves!" },
            {204, "I feel a spurt coming on! " },
            {205, "At this stage I lack worldly knowledge!" },
            {301, "I really appreciate all the cash you're spending on fertilizer!" },
            {302, "That cloud looks just like a vast aggregation of water droplets!" },
            {303, "Have you met my cousin Yggdrasil? Very big in Sweden. Lots of fans." },
            {304, "I'm taking sociology at an online college. I'm really learning a lot." },
            {305, "After careful observation I've deduced that it is the Earth that revolves around the sun and not the reverse as it appears." },
            {401, "When you've been around as long as I have, you sleep less and hallucinate more." },
            {402, "If you're mistaking the forest for the trees, just remember: A forest is a collection of individual trees and not the other way around." },
            {403, "History repeats itself but it always gets the details wrong." },
            {404, "If the past, present and future all simultaneously exist as \"block time\", surely the experience of \"now\" can only be an elaborate illusion?" },
            {405, "Courage is easy; dedication costs extra." },
            {500, "Here's some wisdom that bears repeating..." },
            {600, "Tree food, please!" },
            {800, "Hey, I'm 100 feet tall! Celebrate with me by typing 'daisies' to get the zombies to leave tiny daisies behind when they die." },
            {900, "Whoa! I'm 500 feet tall! This calls for some dancing!  Type 'dance' to get the zombies to boogie on down!" },
            {1000, "WOW! I'm 1000 feet tall! Celebrate with me by typing 'pinata' to make zombies spit out candy when destroyed!" }
        };

        public static string[] zombieNames = new string[]
{
            "Normal",
            "Flag",
            "Cone head",
            "Pole-vaulting",
            "Bucket head",
            "Newspaper",
            "Screen door",
            "Football",
            "Dancing",
            "Backup dancer",
            "Ducky Tube",
            "Snorkel",
            "Zomboni",
            "Bobsled",
            "Dolphin Rider",
            "Jack in the box",
            "Balloon",
            "Digger",
            "Pogo",
            "Yeti",
            "Bungee",
            "Ladder",
            "Catapult",
            "Gargantuar",
            "Imp",
            "Dr. ZomBoss",
            "Pea Head",
            "Wallnut Head",
            "Jalapeno Head",
            "Gatling Head",
            "Squash Head",
            "Tallnut Head",
            "Red Eye Gargantuar",
            "Zombatar",
};

        public static string[] zombieFullDescriptions = new string[]
        {
            "Regular Garden-variety Zombie. Toughness: low. This zombie loves brains. Can't get enough. Brains, brains, brains, day in and night out. Old and stinky brains? Rotten brains? Brains clearly past their prime? Doesn't matter. Regular zombie wants 'em. ",
            "Flag Zombie marks the arrival of a huge pile or \"wave\" of zombies. Toughness: low. Make no mistake, Flag Zombie loves brains. But somewhere down the line he also picked up a fascination with flags. Maybe it's because the flags always have brains on them. Hard to say.",
            "His traffic cone headpiece makes him twice as tough as normal zombies. Toughness: medium. Conehead Zombie shuffled mindlessly forward like every other zombie. But something made him stop, made him pick up a traffic cone and stick it on his head. Oh yeah. He likes to party.",
            "Pole Vaulting Zombie vaults with a pole. Toughness: medium. Speed: fast, then normal (after jump). Special: jumps the first plant he runs into. Some zombies take it further, aspire more, push themselves beyond the normal into greatness. That's Pole Vaulting Zombie right there. That is so him.",
            "His bucket hat makes him extremely resistant to damage. Toughness: high. Weakness: magnet-shroom. Buckethead Zombie always wore a bucket. Part of it was to assert his uniqueness in an uncaring world. Mostly he just forgot it was there in the first place.",
            "His newspaper provides limited defense. Toughness: low. Newspaper Toughness: low. Speed: normal, then fast (after losing newspaper). Newspaper Zombie was *this* close to finishing his sudoku puzzle. No wonder he's freaking out.",
            "His screen door is an effective shield. Toughness: low. Screen Door Toughness: high. Weakness: fume-shroom and magnet-shroom. He got his screen door from the last inexpertly defended home he visited, after he ATE THE HOMEOWNER'S BRAINS.",
            "Football Zombie makes the big plays. Toughness: very high. Speed: fast. Weakness: magnet-shroom. Football Zombie gives 110 percent whenever he's on the field. He's a team player who delivers both offensively and defensively. He has no idea what a football is.",
            "Any resemblance between Dancing Zombie and persons living or dead is purely coincidental. Toughness: medium. Special: summons back-up dancers. Dancing Zombie's latest album, \"GrarrBRAINSarblarbl,\" is already rocketing up the undead charts.",
            "These zombies appear in sets of four whenever Dancing Zombie rocks out. Toughness: low. Backup Dancer Zombie spent six years perfecting his art at the Chewliard Performing Arts School in Zombie New York City.",
            "The ducky tube allows this zombie to float on water. Toughness: low. Only appears in the pool. It takes a certain kind of zombie to be a Ducky Tuber. Not every zombie can handle it. Some crack. They can't take it. They walk away and give up on brains forever.",
            "Snorkel zombies can swim underwater. Toughness: low. Special: submerges to avoid attacks. Only appears in the pool. Zombies don't breathe. They don't need air. So why does Snorkel Zombie need a snorkel to swim underwater?. Answer: peer pressure.",
            "Toughness: high. Special: crushes plants, leaves an ice trail. Weakness: Spikeweed. Not to be mistaken for a Zamboni brand ice resurfacing machine. Zamboni and the image of the ice-resurfacing machine are registered trademarks of Frank J. Zamboni & Co., Inc., and \"Zomboni\" is used with permission. For all your non-zombie related ice resurfacing needs, visit www.zamboni.com!",
            "These zombies appear in teams of four. Toughness: low (each zombie). Bobsled Toughness: low. Special: only appears on ice. Zombie Bobsled Team worked hard to get where they are. They live together, eat brains together and train together to become a cohesive zombie unit.",
            "Dolphin Rider Zombies use dolphins to exploit weaknesses in your pool defense. Toughness: medium. Speed: fast, then slow (after jump). Special: jumps over the first plant he runs into. Only appears in the pool. The dolphin is also a zombie.",
            "This zombie carries an explosive surprise. Toughness: medium. Speed: fast. Special: jack-in-the-box explodes. Weakness: magnet-shroom. This zombie shivers, not because he's cold but because he's crazy.",
            "Balloon Zombie floats above the fray, immune to most attacks. Toughness: low. Special: flying. Weakness: cactus and blover. Balloon Zombie really lucked out. The balloon thing really works and none of the other zombies have picked up on it.",
            "This zombie digs to bypass your defenses. Toughness: medium. Speed: fast, then slow. Special: tunnels underground and appears on the left side of the lawn. Weakness: split pea and magnet-shroom. Digger Zombie spends three days a week getting his excavation permits in order.",
            "Pogo Zombie hops to bypass your defenses. Toughness: medium. Special: hops over plants. Weakness: magnet-shroom. Sproing! Sproing! Sproing! That's the sound of a powerful and effective zombie doing what he does best.",
            "A rare and curious creature. Toughness: high. Special: runs away after a short while. Little is known about the Zombie Yeti other than his name, birth date, social security number, educational history, past work experience and sandwich preference (roast beef and Swiss).",
            "Bungee Zombie attacks from above. Toughness: medium. Special: descends from the sky and steals a plant. Bungee Zombie loves to take risks. After all, what's the point of being dead if you don't live a little?",
            "Ladder Zombie climbs over obstacles. Toughness: medium. Ladder Toughness: medium. Speed: fast, then slow (after placing ladder). Weakness: fume-shroom and magnet-shroom. He picked the ladder up for $8.99.",
            "Catapult Zombie operates heavy machinery. Toughness: medium. Special: lobs basketballs at your plants. Weakness: Spikeweed (pops tyres), Umbrella Leaf (blocks basketballs). Of all the things Catapult Zombie could launch with his catapult, basketballs seemed like the best and most obvious choice.",
            "Gargantuar is a gigantic zombie. Toughness: extremely high. When Gargantuar walks, the earth trembles. When he moans, other zombies fall silent. He is the zombie other zombies dream they could be. But he still can't find a girlfriend. ",
            "Imps are tiny zombies hurled by Gargantuar deep into your defenses. Toughness: low. Imp may be small, but he's wiry. He's proficient in zombie judo, zombie karate and zombie bare-knuckle brawling. He also plays the melodica.",
            "Dr. Zomboss rules them all. Toughness (in Zombot shell): extreme. Edgar George Zomboss achieved his Doctorate in Thanatology in only two years. Quickly mastering thanatological technology, he built his fearsome Zombot and set about establishing absolute dominance of his local subdivision.",
        };

        //For iZombie gamemodes
        public static int[] iZombieNameIndex = new int[] { 0,2,3,4,21,17,20,7,16,6,12,18,8,23,24 };
        public static int[] iZombieSunCosts = new int[] { 50, 75, 75, 125, 150, 125, 125, 175, 150, 100, 175, 200, 350, 300, 50 };

        public static bool[] SeeingStars = new bool[45]
        {
            false,  false,  false,  true,   false,  false,  false,  false,  false,
            false,  false,  false,  true,   true,   false,  false,  false,  false,
            false,  true,   true,   true,   true,   true,   true,   false,  false,
            false,  false,  false,  true,   true,   true,   false,  false,  false,
            false,  false,  false,  true,   false,  false,  true,   false,  false
        };
    }

    enum GameMode
    {
        Adventure,
        SurvivalDay,
        SurvivalNight,
        SurvivalPool,
        SurvivalFog,
        SurvivalRoof,
        SurvivalHardDay,
        SurvivalHardNight,
        SurvivalHardPool,
        SurvivalHardFog,
        SurvivalHardRoof,
        SurvivalEndless1,
        SurvivalEndless2,
        SurvivalEndless3,
        SurvivalEndless4,
        SurvivalEndless5,
        ZomBotany,
        WallnutBowling,
        SlotMachine,
        ItsRainingSeeds,
        Beghouled,
        Invisighoul,
        SeeingStars,
        Zombiquarium,
        BeghouledTwist,
        BigTroubleLittleZombie,
        PortalCombat,
        ColumnLikeYouSeeEm,
        BobsledBonanza,
        ZombieNimbleZombieQuick,
        WhackAZombie,
        LastStand,
        ZomBotany2,
        WallnutBowling2,
        PogoParty,
        DrZombossRevenge,
        LimboArtChallengeWallnut,
        LimboSunnyDay,
        LimboUnsodded,
        LimboBigTime,
        LimboArtChallengeSunflower,
        LimboAirRaid,
        LimboIceLevel,
        ZenGarden,
        LimboHighGravity,
        LimboGraveDanger,
        LimboCanYouDigIt,
        LimboDarkStormyNight,
        LimboBungeeBlitz,
        LimboIntro,
        TreeOfWisdom,
        VaseBreaker1,
        VaseBreaker2,
        VaseBreaker3,
        VaseBreaker4,
        VaseBreaker5,
        VaseBreaker6,
        VaseBreaker7,
        VaseBreaker8,
        VaseBreaker9,
        VaseBreakerEndless,
        IZombie1,
        IZombie2,
        IZombie3,
        IZombie4,
        IZombie5,
        IZombie6,
        IZombie7,
        IZombie8,
        IZombie9,
        IZombieEndless,
        UpsellTest,
        Intro,
        NumGameModes
    };

    enum ChosenSeedState
    {
        FlyingToBank = 0,
        InBank = 1,
        FlyingToChooser = 2,
        InChooser = 3,
        Hidden = 4
    };

    enum ZombieType
    {
        Invalid = -1,
        Normal,
        Flag,
        ConeHead,
        PoleVaulting,
        BucketHead,
        Newspaper,
        ScreenDoor,
        Football,
        Dancing,
        BackupDancer,
        DuckyTube,
        Snrokel,
        Zomboni,
        Bobsled,
        DolphinRider,
        JackInTheBox,
        Balloon,
        Digger,
        Pogo,
        Yeti,
        Bungee,
        Ladder,
        Catapult,
        Gargantuar,
        Imp,
        DrZomBoss,
        PeaHead,
        WallnutHead,
        JalapenoHead,
        GatlingHead,
        SquashHead,
        TallnutHead,
        RedeyeGargantuar,
        NumZombieTypes,
        CachedPolevaulterWithPole,
        CachedZombieTypes
    };

    enum CoinType
    {
        None,
        Silver,
        Gold,
        Diamond,
        Sun,
        Smallsun,
        Largesun,
        FinalSeedPacket,
        Trophy,
        Shovel,
        Almanac,
        Carkeys,
        Vase,
        WateringCan,
        Taco,
        Note,
        UsableSeedPacket,
        PresentPlant,
        AwardMoneyBag,
        AwardPresent,
        AwardBagDiamond,
        AwardSilverSunflower,
        AwardGoldSunflower,
        Chocolate,
        AwardChocolate,
        PresentMinigames,
        PresentPuzzleMode,
        PresentSurvivalMode
    };

    enum GridItemType
    {
        None = 0,
        Gravestone = 1,
        Crater = 2,
        Ladder = 3,
        PortalCircle = 4,
        PortalSquare = 5,
        Brain = 6,
        Vase = 7,
        Squirrel = 8,
        ZenTool = 9,
        Stinky = 10,
        Rake = 11,
        IzombieBrain = 12
    }

    enum CursorType
    {
        Normal,
        PlantFromBank,
        PlantFromUsableCoin,
        PlantFromGlove,
        PlantFromDuplicator,
        PlantFromWheelBarrow,
        Shovel,
        Hammer,
        CobcannonTarget,
        WateringCan,
        Fertilizer,
        BugSpray,
        Phonograph,
        Chocolate,
        Glove,
        MoneySign,
        Wheeelbarrow,
        TreeFood
    }

    enum LevelType
    {
        Normal,
        Night,
        Pool,
        PoolNight,
        Roof,
        Boss //Probably more
    }

    enum ZenPlantNeed
    {
        None = 0,
        Water = 1,
        Fertilizer = 2,
        BugSpray = 3,
        Phonograph = 4
    }


    enum InputMode
    {
        Unknown,
        MainMenu,
        MinigameMenu,
        AwardScreen,
        SeedPicker,
        OnBoard
    }

    enum GameScene
    {
        Loading,
        MainMenu,
        SeedPicker,
        Board,
        BrainEaten,
        AwardScreen,
        Credits,
        MinigameSelector
    }

    enum MowerType
    {
        LawnMower,
        PoolCleaner,
        RoofSweeper,
        TrickedOut
    }
    
    public static class DialogIDs
    {
        public const int NewGame = 0;
        public const int Options = 1;
        public const int NewOptions = 2;
        public const int Almanac = 3;
        public const int Store = 4;
        public const int PregameNag = 5;
        public const int LoadGame = 6;
        public const int ConfirmUpdateCheck = 7;
        public const int CheckingUpdates = 8;
        public const int RegisterError = 9;
        public const int ColorDepthExp = 10;
        public const int OpenUrlWait = 11;
        public const int OpenUrlFail = 12;
        public const int Quit = 13;
        public const int HighScores = 14;
        public const int Nag = 15;
        public const int Info = 16;
        public const int GameOver = 17;
        public const int LevelComplete = 18;
        public const int Paused = 19;
        public const int NoMoreMoney = 20;
        public const int Bonus = 21;
        public const int ConfirmBackToMain = 22;
        public const int ConfirmRestart = 23;
        public const int ThanksForRegistering = 24;
        public const int NotEnoughMoney = 25;
        public const int Upgraded = 26;
        public const int NoUpgrade = 27;
        public const int ChooserWarning = 28;
        public const int UserDialog = 29;
        public const int CreateUser = 30;
        public const int ConfirmDeleteUser = 31;
        public const int RenameUser = 32;
        public const int CreateUserError = 33;
        public const int RenameUserError = 34;
        public const int Cheat = 35;
        public const int CheatError = 36;
        public const int Continue = 37;
        public const int GetReady = 38;
        public const int RestartConfirm = 39;
        public const int ConfirmPurchase = 40;
        public const int ConfirmSell = 41;
        public const int TimesUp = 42;
        public const int VirtualHelp = 43;
        public const int JumpAhead = 44;
        public const int CrazyDave = 45;
        public const int StorePurchase = 46;
        public const int ZenSell = 47;
        public const int CreditsPaused = 48;
        public const int Imitater = 49;
        public const int PurchasePacketSlot = 50;

        public const int unkDialog1 = 51; 
        public const int ZombatarLicense = 52;
        public const int unkDialog2 = 53;
        public const int unkDialog3 = 54;
        public const int unkDialog4 = 55;
        public const int steamCloudSavingActive = 56;
        public const int stamCloudLocalChoice = 57;
        //How many more are there?
    }

    

}
