# Basket Option Pricing Engine

**Advanced C# pricing engine for multi-asset derivative products**

*Master 2 Financial Engineering - Université Paris-Dauphine PSL*  
*Rémi Schmitt & Théo Verdelhan - January 2026*

---

## Overview

Professional-grade basket option pricer implementing both analytical approximations and Monte Carlo simulation with real market data integration (ECB €STR rates, Bloomberg volatility surfaces).

### Core Technologies

**C# / .NET 9.0** | **Quantitative Finance** | **Numerical Methods** | **Object-Oriented Design**

### Key Features

✓ **Moment Matching Pricer** - Fast analytical approximation (Brigo et al. method)  
✓ **Monte Carlo Engine** - Reference pricing with 98.6% variance reduction via control variates  
✓ **Dual Framework** - H1 (constant parameters) and H2 (term structure modeling)  
✓ **Real Market Data** - ECB €STR rates and Bloomberg OVDV volatility surfaces  
✓ **Production-Ready** - 15 automated tests validating mathematical and economic properties

### Performance

| Method | Speed | Accuracy |
|--------|-------|----------|
| Moment Matching | < 1ms | Excellent for standard cases |
| Monte Carlo (1M sims) | ~500ms | Reference-grade with variance reduction |

---

## Technical Implementation

### Architecture

Clean object-oriented design following quantitative finance best practices:

```
BasketOptionPricer/
├── Models/           Stock, Basket, BasketOption, Term structures
├── Pricers/          MomentMatching (H1/H2), MonteCarlo
├── Utils/            Mathematical functions (Normal CDF, Black-Scholes)
├── Data/             Market data loaders (CSV parsing)
└── Tests/            Unit tests (7) + Functional tests (8)
```

### Mathematical Models

**H1 Framework** - Black-Scholes with constant parameters:
- Geometric Brownian motion: `dSᵢ = (r-qᵢ)Sᵢdt + σᵢSᵢdWᵢ`
- Analytical moment matching for fast pricing
- Full correlation matrix support

**H2 Framework** - Deterministic term structures:
- Time-varying rates: `r(t)` and volatilities: `σᵢ(t)`
- Numerical integration via trapezoidal rule
- Linear interpolation between term points

**Monte Carlo Engine**:
- Cholesky decomposition for correlated paths
- Log-Euler discretization scheme (252+ steps/year)
- Control variate variance reduction (geometric mean basket)

### Key Algorithms Implemented

1. **Cholesky Decomposition** - O(n³) correlation matrix factorization
2. **Trapezoidal Integration** - Numerical term structure integration
3. **Normal CDF** - Abramowitz & Stegun approximation (|error| < 1.5×10⁻⁷)
4. **Moment Matching** - Lognormal basket approximation calibrated to first two moments

---

## Results & Validation

### Numerical Accuracy

**H1 Pricing Example** (3-asset basket, 1Y maturity, €STR 1.933%):
- Call option (K=107.10€): **4.89€**
- Put option (K=96.90€): **3.75€**

**H2→H1 Convergence**: Perfect match (0.0000% error) when term structures are flat

**Monte Carlo Variance Reduction**:
```
Standard MC:        SE = 0.0568  
With Control Var:   SE = 0.0066  →  98.6% variance reduction
```

### Comprehensive Testing

**Unit Tests** (7):
- Mathematical functions (Normal CDF, Black-Scholes)
- Object construction and validation
- Strike monotonicity and boundary conditions

**Functional Tests** (8):
- Multi-asset basket configurations
- Framework convergence validation
- Correlation sensitivity analysis
- Put-call relationship consistency

All tests include tolerance checks and economic property validation.

---

## Usage

### Quick Start

```bash
dotnet build -c Release
dotnet run -c Release
```

### Interactive Interface

User-friendly menu system for:
1. **Auto Demo** - Pre-configured H1/H2 comparison scenarios
2. **Interactive Mode** - Custom basket configuration wizard
3. **Test Suite** - Automated validation (unit + functional)
4. **Bloomberg Data** - Volatility surface analysis

### Example: Custom Pricing

```
🎯 Configure basket (2-10 assets)
💰 Input market parameters (rates, correlations, volatilities)
📋 Define option (Call/Put, Strike, Maturity)
⚡ Price with Moment Matching or Monte Carlo
📊 Get results with confidence intervals
```

---

## Market Data Integration

**Risk-Free Rate**: €STR (Euro Short-Term Rate) from ECB  
- Real historical data: Oct 2019 - Jan 2026
- Current rate: 1.933% (Jan 23, 2026)

**Volatility Surfaces**: Bloomberg OVDV for Euro Stoxx 50  
- Multiple maturities and moneyness points
- Linear interpolation for continuous surface

---

## Technical Skills Demonstrated

**Programming**: C# 9.0, .NET SDK, OOP design patterns, LINQ

**Finance**: Derivative pricing, Black-Scholes model, Monte Carlo methods, variance reduction techniques, term structure modeling

**Mathematics**: Stochastic calculus, numerical integration, matrix decomposition, statistical estimation

**Software Engineering**: Unit testing, functional testing, input validation, error handling, clean code architecture

**Data**: CSV parsing, real market data integration (ECB, Bloomberg)

---

## References

**Academic**: Brigo et al. (2004) - Moment matching for basket options | Glasserman (2003) - Monte Carlo methods in finance

**Data Sources**: European Central Bank (€STR rates) | Bloomberg Terminal (OVDV surfaces)

---

## Quick Reference

```bash
# Build optimized version
dotnet build -c Release

# Run application
dotnet run -c Release

# Test suite
dotnet run → Option 3 (Unit) / Option 4 (Functional)
```

**Project Structure**: 1000+ lines of C# across Models, Pricers, Utils, Tests  
**Test Coverage**: 15 automated tests (100% pass rate)  
**Performance**: Release mode 2-3× faster than Debug

---

*M2 Quantitative Finance Project - Université Paris-Dauphine PSL*  
*Full technical documentation available in `ReproductionParameters.md`*
