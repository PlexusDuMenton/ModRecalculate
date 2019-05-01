# Random Spices
#### by PlexusDuMenton

Some little change I made to the game to goes by my way, many more change will come, all being disable/enable easily ^^


Exponential Health scale so no longer Late Game OneShot

Shield Item now scale with player level




###Formula

Health = BaseHealth + HealthGainPerLevel * (Level-1)^e_value + Other_Health_Gain;

e_value is 1.5, but can be edited in the config file (set to 1 to disable)

Shield = ShieldCount * (25 + ShieldBoost_value*(Level-1)) + Other Shield source

ShieldBoost_value is 5, but can be changed in config file (set to 0 to disable)

Compatible with character customizer mod




###Changelog

0.1.3 : Fixed Shield not properly scaling, Ennemies no longer get exponential health too ^^'

0.1.2 : Fix bug where health get maxed when using any skill or sprinting

0.1.1 : Fixed bug where actual hp weren't scaled to max health