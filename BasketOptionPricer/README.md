# Pricing d'Options Basket

**Application C# pour calculer le prix d'options sur panier d'actifs**

## Comment l'utiliser

1. **Installer .NET 9.0**
2. **Lancer le programme :**
   ```bash
   dotnet run
   ```
3. **Choisir dans le menu :**
   - `1` → Calcul rapide (Moment Matching)
   - `2` → Calcul précis (Monte Carlo) 
   - `3` → Tests unitaires
   - `4` → Tests fonctionnels
   - `5` → Quitter

## Exemple simple

```
2 actifs : Apple (100€) et Google (110€)
Option d'achat à 105€ dans 1 an
→ Prix calculé : ~13.89€
```

## Fichiers principaux

- `Program.cs` → Programme principal
- `Models/` → Données des actifs et options
- `Pricers/` → Calculs de prix
- `Tests/` → Vérifications automatiques

## Méthodes

- **H1** : Rapide et précis pour la plupart des cas
- **H2** : Plus lent mais très précis (1 million de simulations)

---
*Projet de M2 - Dauphine - Verdelhan & Schmitt*