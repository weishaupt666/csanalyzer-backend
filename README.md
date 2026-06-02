Dokumentacja Backend - CSAnalyzer
1. Opis Projektu
CSAnalyzer to usługa oparta na WCF (Windows Communication Foundation), która działa jako agregator danych. Pobiera statystyki graczy CS2 z oficjalnych API Steam oraz Faceit, a następnie łączy je w jeden spójny obiekt JSON, który jest zwracany do frontendu.

2. Stos Technologiczny
Język: C#

Framework: .NET Framework 4.7.2/4.8

Technologia: WCF Service Application (skonfigurowana jako REST przez webHttpBinding)

Biblioteki zewnętrzne: Newtonsoft.Json do parsowania odpowiedzi z zewnętrznych API.

3. Konfiguracja (Klucze API)
Usługa wymaga autoryzacji do zewnętrznych API. Klucze należy przechowywać w pliku secrets.config (w głównym katalogu projektu), który jest wykluczony z systemu kontroli wersji:

XML
<appSettings>
  <add key="SteamApiKey" value="TWÓJ_KLUCZ_STEAM" />
  <add key="FaceitApiKey" value="TWÓJ_KLUCZ_FACEIT" />
</appSettings>
4. Endpoint API
Pobieranie profilu gracza
Metoda HTTP: GET

Ścieżka: /CSService.svc/profile/{steamId}

Parametr url: steamId (string) — 64-bitowy identyfikator profilu Steam.

Odpowiedź: application/json

5. Przepływ działania (Logika biznesowa)
Odbiór żądania: Endpoint WCF przyjmuje steamId i inicjuje obiekt modelu PlayerProfile.

Integracja ze Steam API: Serwer wykonuje serię równoległych/kolejnych żądań HTTP (protokół HTTP) do pobrania:

Podstawowych danych profilu (avatar, nick, data utworzenia konta).

Statusu blokad (VAC Ban, Trade Ban).

Czasu gry w CS2 (całkowitego i z ostatnich 2 tygodni).

Poziomu profilu i liczby znajomych.

Integracja z Faceit API: Serwer wykorzystuje protokół HTTPS (wymuszony TLS 1.2 w statycznym konstruktorze), aby pobrać:

faceit_elo i podstawowy profil powiązany ze steamId.

Szczegółowe statystyki z endpointu /stats/cs2 (korzystając z wyodrębnionego wcześniej player_id), w tym Winrate, K/D, % HS i ostatnie wyniki.

Zwrócenie wyniku: Zebrane dane są mapowane na właściwości kontraktu danych PlayerProfile i automatycznie serializowane do formatu JSON.

Obsługa błędów: Jeśli profil jest prywatny lub gracz nie ma konta Faceit, usługa ignoruje brakujące dane (zwraca null) zamiast zgłaszać wyjątek (crash), a w razie krytycznych błędów HTTP wypełnia pole error. Zaimplementowano obsługę CORS w Global.asax.

