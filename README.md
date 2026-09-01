# CLI RPG - Terres d'Arras

[![CI](https://github.com/pierre3150/cli-rpg/actions/workflows/ci.yml/badge.svg)](https://github.com/pierre3150/cli-rpg/actions/workflows/ci.yml)

RPG en ligne de commande en **C#/.NET 8** : systeme de classes, magie, boutique, combats tour par tour, et un catalogue de monstres/sorts/objets **synchronise en base** (EF Core) plutot que codé en dur dans le moteur de jeu.

## Stack technique

- **.NET 8**, architecture en 3 projets : `RpgGame.Core` (moteur, testable, zero dependance console), `RpgGame` (interface CLI), `RpgGame.Core.Tests` (xUnit)
- **Entity Framework Core + SQLite** pour la sauvegarde et le catalogue
- **Synchronisation idempotente du catalogue** (`SeedData.SyncAsync`) : au demarrage, les monstres/sorts/objets sont upsertes par nom - modifier l'equilibrage dans le code met a jour les parties existantes sans effacer la progression du joueur
- **xUnit**, logique de combat/level-up/boutique testee avec une source d'aleatoire injectable (`IRandomProvider`) pour des tests deterministes (coups critiques inclus)
- **Docker** multi-stage, image publiee sur GitHub Container Registry a chaque merge sur `main`

## Systeme de jeu

### Classes
| Classe | Points forts | Sorts |
|---|---|---|
| Guerrier | Force, vitalite | Cri de guerre (buff) |
| Mage | Intelligence, degats magiques eleves, peu de PV | Boule de feu, Eclair, Soin |
| Voleur | Agilite, taux de coup critique eleve | Poison lame |
| Clerc | Equilibre, soins puissants | Frappe sacree, Soin, Soin superieur |

### Combat
Tour par tour : Attaquer / Lancer un sort / Utiliser un objet / Fuir. Les degats prennent en compte l'arme equipee, la defense adverse (armure comprise), et un taux de coup critique qui depend de l'agilite (plus eleve pour le Voleur).

### Progression
Courbe XP quadratique (`20*niveau² + 30*niveau`). Chaque niveau augmente les 4 statistiques (avec un profil de croissance different par classe) et les PV/MP max, sans soigner completement le joueur si le level-up arrive en plein combat.

### Boss
Deux boss actuellement : **Le Roi Gobelin** (Nv.6) et le **Dragon des Cimes** (Nv.12), accessibles depuis le menu principal.

## Lancer en local

```bash
dotnet restore
dotnet run --project src/RpgGame
```
La sauvegarde est un fichier `rpg.db` (SQLite) cree a cote de l'executable. Un seul personnage actif a la fois pour l'instant (rechargé automatiquement au lancement suivant).

## Lancer via Docker

```bash
docker run -it -v rpg-save:/data ghcr.io/pierre3150/cli-rpg:latest
```
Le volume `rpg-save` persiste la sauvegarde entre deux lancements du conteneur.

## Tests

```bash
dotnet test
```

## Idees d'extension

- Classes secondaires / multiclassage
- Equipement avec emplacements (arme + armure simultanes, actuellement le premier objet du bon type trouve dans l'inventaire compte)
- Effets de statut (poison, etourdissement) au-dela du cosmetique actuel
- Donjons a plusieurs combats enchaines avant un boss
- Sauvegardes multiples / plusieurs personnages
