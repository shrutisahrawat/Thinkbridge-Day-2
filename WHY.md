# Why a Rich Domain Model?

Refactoring the `Quote` entity from anemic to rich shifted our architecture from a passive data structure to an active guardian of business rules. 

### What the Rich Model Bought Us
1. **Encapsulation & Invariant Protection:** By making setters private and introducing the static factory method `Quote.Create(author, text)`, it became impossible to instantiate an invalid quote. The rules for length limits (1-200 for author, 1-1000 for text) now live inside the entity where they belong, rather than being scattered across controllers or services.
2. **Immutability:** Once a quote is created, its core content (`Text`) cannot be mutated directly. The only permitted state change is a controlled soft-delete action (`SoftDelete()`), protecting the system's auditing integrity.

### The Bug Scenario Caught by the Rich Model
With the anemic version, a controller could accidentally accept an empty string or a 5,000-character spam payload and save it straight to the database via public setters (`quote.Text = incomingText`). Furthermore, downstream logic could overwrite quotes arbitrarily. The rich model completely prevents this by throwing an exception upfront during construction, ensuring bad state never enters the system.
