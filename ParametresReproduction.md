# Paramètres de Reproduction des Résultats

Ce fichier contient les jeux de paramètres exacts utilisés dans les démonstrations automatiques du projet, permettant de reproduire précisément les résultats obtenus.

---

## 0. Courbe de Taux Sans Risque

### Source des données
| Élément | Valeur |
|---------|--------|
| **Référence** | €STR (Euro Short-Term Rate) |
| **Source** | Banque Centrale Européenne (BCE) |
| **Code série** | EST.B.EU000A2X2A25.WT |
| **Type** | Volume-weighted trimmed mean rate |
| **Date de pricing** | 23 janvier 2026 |
| **Taux overnight** | **1.933%** |

### Justification du choix
L'€STR est le taux de référence sans risque officiel de la zone euro depuis octobre 2019, publié quotidiennement par la BCE. Il remplace l'EONIA et représente le coût de l'emprunt overnight non garanti sur le marché interbancaire européen.

### Fichier source
- `ECB Data Portal_20260126121402.csv` - Historique €STR du 01/10/2019 au 23/01/2026

### Utilisation dans le code
```csharp
// Taux sans risque €STR - BCE au 23/01/2026
double riskFreeRate = 0.01933; // 1.933%

// Ou via DeterministicRateModel pour l'approche H2
var rateModel = new DeterministicRateModel(0.01933);
```

### Note importante
L'€STR est un taux overnight. Pour des maturités plus longues, une courbe OIS EUR complète serait plus précise. Cependant, pour des options de maturité ≤ 1 an avec un environnement de taux stable, l'utilisation du taux €STR constant est une approximation acceptable.

---

## 1. Démonstration H1 - Moment Matching

### Paramètres d'entrée :
```
Nombre d'actifs : 2
Actif 1 : S0 = 100.0, σ = 0.20
Actif 2 : S0 = 110.0, σ = 0.25
Corrélation ρ12 = 0.30
Taux sans risque r = 0.01933 (€STR BCE 23/01/2026)
Strike K = 105.0
Maturité T = 1.0 année
Type d'option : Call
```

### Résultats attendus :
- **Prix H1** : ~11.50
- **Moments calculés** : μ_B ≈ 111.37, σ_B ≈ 25.13
- **Temps de calcul** : < 1ms

## 2. Démonstration H2 - Monte Carlo Optimisé

### Paramètres d'entrée :
```
Nombre d'actifs : 2
Actif 1 : S0 = 100.0, σ = 0.20  
Actif 2 : S0 = 110.0, σ = 0.25
Corrélation ρ12 = 0.30
Taux sans risque r = 0.01933 (€STR BCE 23/01/2026)
Strike K = 105.0
Maturité T = 1.0 année
Type d'option : Call
Simulations : 1,000,000
```

### Résultats attendus :
- **Prix H2** : ~11.49 ± 0.05
- **Réduction de variance** : ~40-60%
- **Convergence** : Écart H1/H2 < 1%
- **Temps de calcul** : ~500-1000ms

## 3. Tests Unitaires - Paramètres de Validation

### Test CDF Normale :
```
Φ(0) = 0.5000 (attendu : 0.5000)
Φ(1.96) = 0.9750 (attendu : 0.9750)
Φ(-1.96) = 0.0250 (attendu : 0.0250)
```

### Test Black-Scholes :
```
S0 = 100, K = 100, r = 0.01933 (€STR), σ = 0.20, T = 1.0
Prix Call attendu : ~8.92
Prix Put attendu : ~6.99
```

### Test Moment Matching :
```
2 actifs identiques : S0 = 100, σ = 0.20, ρ = 0
Strike = 100, r = 0.01933 (€STR), T = 1.0
Prix attendu : ~2 × prix Black-Scholes
```

## 4. Tests Fonctionnels - Scénarios Complets

### Scenario 1 - Basket 2 Actifs ATM :
```
S1 = 100, σ1 = 0.20
S2 = 100, σ2 = 0.20  
ρ = 0.50, K = 100, T = 1.0, r = 0.01933 (€STR)
Résultat attendu : Prix ≈ 9.50
```

### Scenario 2 - Basket 3 Actifs OTM :
```
S1 = 90, σ1 = 0.25
S2 = 95, σ2 = 0.30
S3 = 85, σ3 = 0.35
ρij = 0.20, K = 100, T = 0.5, r = 0.01933 (€STR)
Résultat attendu : Prix < 2.0 (OTM)
```

### Scenario 3 - Test de Convergence :
```
Paramètres identiques H1/H2
Écart relatif attendu : < 1%
```

### Scenario 4 - Test Réduction de Variance :
```
Monte Carlo standard vs optimisé
Réduction attendue : 40-60%
```

## 5. Configuration Système Recommandée

### Paramètres d'exécution :
```
Mode Release pour les performances
Simulations MC : 100K-1M selon précision souhaitée
Graine aléatoire : 12345 (pour reproductibilité)
```
