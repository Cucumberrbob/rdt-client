import { Provider } from '$lib/generated/apiClient';

export function formatProvider(provider: Provider) {
	switch (provider) {
		case Provider.RealDebrid:
			return 'Real Debrid';
		case Provider.AllDebrid:
			return 'All Debrid';
		case Provider.Premiumize:
			return 'Premiumize';
		case Provider.TorBox:
			return 'TorBox';
		case Provider.DebridLink:
			return 'Debrid Link';
		default:
			return 'Unknown Provider';
	}
}
