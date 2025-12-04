# Basket Option Pricing - Verdelhan & Schmitt

## Description

Application C# pour le pricing d'options basket utilisant les approches H1 (Moment Matching) et H2 (Monte Carlo avec techniques de réduction de variance).

## Structure du Projet

```
BasketOptionPricer/
├── Models/                 # Modèles de données
│   ├── Asset.cs           # Classe représentant un actif
│   └── BasketOption.cs    # Classe représentant l'option basket
├── Pricers/               # Moteurs de calcul
│   ├── H1MomentMatchingPricer.cs     # Pricer H1 (Moment Matching)
│   └── H2MonteCarloOptimizedPricer.cs # Pricer H2 (Monte Carlo optimisé)
├── Utils/                 # Utilitaires
│   └── MathUtils.cs      # Fonctions mathématiques (CDF normale, etc.)
├── Interface/            # Interface utilisateur
│   └── UserInterface.cs  # Gestion des interactions utilisateur
├── Tests/                # Tests
│   ├── UnitTests.cs      # Tests unitaires (7 tests)
│   └── FunctionalTests.cs # Tests fonctionnels (8 tests)
└── Program.cs            # Point d'entrée principal
```

## Fonctionnalités

### Méthodes de Pricing
1. **H1 - Moment Matching** : Méthode analytique basée sur Brigo et al.
2. **H2 - Monte Carlo Optimisé** : Simulation avec réduction de variance

### Tests
- **Tests Unitaires** : Validation des composants individuels (100% de réussite)
- **Tests Fonctionnels** : Validation des scénarios end-to-end (100% de réussite)

## Utilisation

### Compilation
```bash
dotnet build
```

### Exécution
```bash
dotnet run
```

### Menu Principal
1. **Pricing H1** - Moment Matching (Brigo et al.)
2. **Pricing H2** - Monte Carlo Optimisé
3. **Tests Unitaires** - Validation des composants
4. **Tests Fonctionnels** - Validation des scénarios
5. **Quitter**

## Tests Disponibles

### Tests Unitaires (7 tests)
- Test de la fonction CDF normale
- Test du modèle Black-Scholes
- Test du pricer Moment Matching
- Test de construction des modèles
- Test de validation des paramètres

### Tests Fonctionnels (8 tests)
- Test basket 2 actifs ATM
- Test basket 3 actifs OTM
- Test de convergence H1/H2
- Test de réduction de variance
- Test de sensibilité aux paramètres

## Paramètres d'Exemple

```
Actif 1: S0=100, σ=0.2, r=0.05
Actif 2: S0=110, σ=0.25, r=0.05
Corrélation: ρ=0.3
Strike: K=105
Maturité: T=1 an
Type: Call
```

## Auteurs

- Verdelhan
- Schmitt

## Version

.NET 9.0